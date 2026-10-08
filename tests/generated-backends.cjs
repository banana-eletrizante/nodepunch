const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const { spawnSync } = require('node:child_process');
const dotenv = require('dotenv');
const jwt = require('jsonwebtoken');
const bcrypt = require('bcryptjs');

const base = path.resolve(process.argv[2]);
const root = fs.readdirSync(base).filter(n => n.startsWith('run-'))
  .map(n => path.join(base, n)).sort((a, b) => fs.statSync(b).mtimeMs - fs.statSync(a).mtimeMs)[0];
assert(root, 'Run the C# generator tests first');
let checks = 0;
function check(ok, label) { assert(ok, label); checks++; }
function load(file, mocks = {}) {
  const mod = { exports: {} };
  const sandbox = { module: mod, exports: mod.exports, Buffer, process, console,
    require(name) { return Object.hasOwn(mocks, name) ? mocks[name] : require(name); } };
  vm.runInNewContext(fs.readFileSync(file, 'utf8'), sandbox, { filename: file });
  return mod.exports;
}
function response() { return { statusCode: 200, status(code) { this.statusCode = code; return this; }, json(data) { this.data = data; return this; } }; }
function walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const file = path.join(dir, entry.name);
    if (entry.isDirectory()) walk(file);
    else if (entry.name.endsWith('.js')) {
      const result = spawnSync(process.execPath, ['--check', file], { encoding: 'utf8' });
      assert.equal(result.status, 0, result.stderr); checks++;
    }
  }
}

(async () => {
  for (const name of ['none', 'mysql', 'mysql-procedures', 'postgres', 'firebase']) walk(path.join(root, name));
  const noDotenv = { config() {} };
  process.env.JWT_SECRET = 'test-only-key-'.repeat(6);
  process.env.JWT_EXPIRES_IN = '1h';
  for (const name of ['mysql', 'mysql-procedures', 'postgres', 'firebase']) {
    const dir = path.join(root, name);
    if (name !== 'firebase') check(dotenv.parse(fs.readFileSync(path.join(dir, '.env'))).DB_PASSWORD === ' senha#com espaço ', 'password preserves # and whitespace');
    let account;
    let calls = 0;
    const banco = {
      async consultarSql(sql, args) { return this.consultar(sql, args); },
      async consultar(sql, args) {
        calls++;
        if (sql === 'usuarios') return account ? [account] : [];
        assert(!sql.startsWith('CALL'), 'Auth must execute raw SQL, not a procedure');
        if (sql.startsWith('INSERT')) {
          if (account) throw Object.assign(new Error('Duplicate'), { code: name === 'postgres' ? '23505' : 'ER_DUP_ENTRY' });
          account = { id: 1, email: args[0], senha: args[1] };
          return name === 'postgres' ? [account] : { insertId: 1 };
        }
        return account && account.email === args[0] ? [account] : [];
      },
      db: { collection() { return { doc(id) { return { id, async create(data) {
        if (account) throw Object.assign(new Error('Already exists'), { code: 6 });
        account = { id, ...data };
      } }; } }; } }
    };
    const Controller = load(path.join(dir, 'src/controllers/authController.js'), { '../base/Banco': banco, dotenv: noDotenv });
    const before = calls;
    for (const body of [{ email: 12, senha: 'abcdefgh' }, { email: 'invalid', senha: 'abcdefgh' }, { email: 'test@example.com', senha: 'short' }, { email: 'test@example.com', senha: 'á'.repeat(40) }]) {
      const res = response(); await Controller.registrar({ body }, res);
      check(res.statusCode === 400, 'reject invalid signup for ' + name);
    }
    check(calls === before, 'invalid signup never reaches database');
    const signup = response();
    await Controller.registrar({ body: { email: 'test@example.com', senha: 'abcdefgh' } }, signup);
    check(signup.statusCode === 201, 'signup works for ' + name);
    check(await bcrypt.compare('abcdefgh', account.senha), 'stored password is hashed');
    check(jwt.verify(signup.data.token, process.env.JWT_SECRET).id === account.id, 'JWT includes inserted account id');
    const duplicate = response();
    await Controller.registrar({ body: { email: 'test@example.com', senha: 'abcdefgh' } }, duplicate);
    check(duplicate.statusCode === 409, 'duplicate email returns conflict');
    for (const [senha, status] of [['wrong-password', 401], ['abcdefgh', 200], [42, 400]]) {
      const login = response();
      await Controller.login({ body: { email: 'test@example.com', senha } }, login);
      check(login.statusCode === status, 'login result for ' + name);
    }
    if (name === 'firebase') {
      account = undefined;
      const first = response(); const second = response();
      await Promise.all([Controller.registrar({ body: { email: 'race@example.com', senha: 'abcdefgh' } }, first),
        Controller.registrar({ body: { email: 'race@example.com', senha: 'abcdefgh' } }, second)]);
      check([first.statusCode, second.statusCode].sort().join(',') === '201,409', 'concurrent Firebase signup creates only one account');
      account = { id: signup.data.usuario.id, email: 'test@example.com', senha: account.senha };
    }
    const middleware = load(path.join(dir, 'src/middleware/auth.js'), { dotenv: noDotenv });
    for (const header of ['Basic ' + signup.data.token, 'Bearer invalid', undefined, ['bad']]) {
      const res = response(); let passed = false;
      middleware({ headers: { authorization: header } }, res, () => passed = true);
      check(!passed && [401, 403].includes(res.statusCode), 'invalid auth header blocked');
    }
    let passed = false;
    const req = { headers: { authorization: 'Bearer ' + signup.data.token } };
    middleware(req, response(), () => passed = true);
    check(passed && req.usuario.id === account.id, 'Bearer token accepted');
    if (name.startsWith('mysql')) {
      let rawSql;
      const Banco = load(path.join(dir, 'src/base/Banco.js'), { dotenv: noDotenv, 'mysql2/promise': { createPool() { return { async execute(sql) { rawSql = sql; return [[{ id: 1 }]]; } }; } } });
      await Banco.consultarSql('SELECT id FROM usuarios');
      check(rawSql === 'SELECT id FROM usuarios', 'actual generated Banco runs raw SQL in ' + name);
      if (name === 'mysql-procedures') {
        await assert.rejects(Banco.consultar('proc); DROP TABLE usuarios; --'), /Nome de procedure inválido/);
        checks++;
      }
    }
  }

  // Start the actual generated Express app with its actual middleware and HTTP requests.
  const dir = path.join(root, 'none');
  let server;
  const express = require('express');
  function expressForTest() {
    const app = express();
    const original = app.listen.bind(app);
    app.listen = (_port, callback) => { server = original(0, '127.0.0.1', callback); return server; };
    return app;
  }
  expressForTest.json = express.json;
  load(path.join(dir, 'server.js'), { express: expressForTest, dotenv: noDotenv,
    './src/config/cors': load(path.join(dir, 'src/config/cors.js')),
    './src/routes/produtosRoutes': require('express').Router(),
    './src/routes/exemploRoutes': require('express').Router() });
  try {
    if (!server.listening) await new Promise(resolve => server.once('listening', resolve));
    const url = 'http://127.0.0.1:' + server.address().port;
    const health = await fetch(url + '/health');
    check(health.status === 200 && (await health.json()).ok, 'actual /health endpoint');
    check(health.headers.has('x-content-type-options'), 'Helmet middleware enabled');
    check((await fetch(url + '/missing')).status === 404, 'unknown route is 404');
    check((await fetch(url + '/produtos', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{bad' })).status === 400, 'malformed JSON is 400, not 500');
  } finally { await new Promise(resolve => server.close(resolve)); }
  console.log(`Passed ${checks} generated-JavaScript and HTTP checks.`);
})().catch(error => { console.error(error); process.exitCode = 1; });
