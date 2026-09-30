using System;

namespace NodePunch.Core
{
    internal static partial class Funcoes
    {
        private static string GerarBancoMySQL(ConexaoBanco dados)
        {
            string consultarExecutar = dados.ComSP
                ? tab + "static async consultar(nomeProcedure, parametros = []) {\n" + tab + tab + "await Banco.#conectar();\n" + tab + tab + "const placeholders = parametros.map(() => '?').join(', ');\n" + tab + tab + "const sql = placeholders ? `CALL ${nomeProcedure}(${placeholders})` : `CALL ${nomeProcedure}`;\n" + tab + tab + "const [linhas] = await Banco.#pool.query(sql, parametros);\n" + tab + tab + "return linhas[0] || [];\n" + tab + "}\n\n" + tab + "static async executar(nomeProcedure, parametros = []) {\n" + tab + tab + "await Banco.#conectar();\n" + tab + tab + "const placeholders = parametros.map(() => '?').join(', ');\n" + tab + tab + "const sql = placeholders ? `CALL ${nomeProcedure}(${placeholders})` : `CALL ${nomeProcedure}`;\n" + tab + tab + "await Banco.#pool.query(sql, parametros);\n" + tab + "}\n"
                : tab + "static async consultar(comando, parametros = []) {\n" + tab + tab + "await Banco.#conectar();\n" + tab + tab + "const [linhas] = await Banco.#pool.execute(comando, parametros);\n" + tab + tab + "return linhas;\n" + tab + "}\n\n" + tab + "static async executar(comando, parametros = []) {\n" + tab + tab + "await Banco.#conectar();\n" + tab + tab + "await Banco.#pool.execute(comando, parametros);\n" + tab + "}\n";

            return "const mysql = require('mysql2/promise');\nrequire('dotenv').config();\n\nclass Banco {\n" +
tab + "static #pool = null;\n\n" +
tab + "static async #conectar() {\n" +
tab + tab + "if (Banco.#pool !== null) return;\n" +
tab + tab + "Banco.#pool = mysql.createPool({ host: process.env.DB_HOST, port: process.env.DB_PORT || " + (string.IsNullOrWhiteSpace(dados.Porta) ? "3306" : dados.Porta) + ", user: process.env.DB_USER, password: process.env.DB_PASSWORD, database: process.env.DB_NAME, charset: 'utf8mb4', waitForConnections: true, connectionLimit: 10 });\n" +
tab + "}\n\n" + consultarExecutar + "}\n\nmodule.exports = Banco;\n";
        }

        private static string GerarBancoPostgreSQL(ConexaoBanco dados)
        {
            return "const { Pool } = require('pg');\nrequire('dotenv').config();\n\nclass Banco {\n" +
tab + "static #pool = null;\n" +
tab + "static #conectar() { if (Banco.#pool !== null) return; Banco.#pool = new Pool({ host: process.env.DB_HOST, port: process.env.DB_PORT || " + (string.IsNullOrWhiteSpace(dados.Porta) ? "5432" : dados.Porta) + ", user: process.env.DB_USER, password: process.env.DB_PASSWORD, database: process.env.DB_NAME, max: 10 }); }\n" +
tab + "static async consultar(comando, parametros = []) { Banco.#conectar(); const resultado = await Banco.#pool.query(comando, parametros); return resultado.rows; }\n" +
tab + "static async executar(comando, parametros = []) { Banco.#conectar(); await Banco.#pool.query(comando, parametros); }\n" +
"}\nmodule.exports = Banco;\n";
        }

        private static string GerarBancoFirebase(ConexaoBanco dados)
        {
            return "const admin = require('firebase-admin');\nconst path = require('path');\nrequire('dotenv').config();\n\nclass Banco {\n" +
tab + "static #app = null;\n" +
tab + "static #db = null;\n" +
tab + "static #inicializar() { if (Banco.#app !== null) return; const serviceAccount = require(path.resolve(process.env.FIREBASE_SERVICE_ACCOUNT)); Banco.#app = admin.initializeApp({ credential: admin.credential.cert(serviceAccount), projectId: process.env.FIREBASE_PROJECT_ID }); Banco.#db = admin.firestore(); }\n" +
tab + "static get db() { Banco.#inicializar(); return Banco.#db; }\n" +
tab + "static async consultar(colecao, filtros = []) { Banco.#inicializar(); let ref = Banco.#db.collection(colecao); filtros.forEach(([campo, operador, valor]) => { ref = ref.where(campo, operador, valor); }); const snapshot = await ref.get(); return snapshot.docs.map(doc => ({ id: doc.id, ...doc.data() })); }\n" +
tab + "static async executar(colecao, dados, id = null) { Banco.#inicializar(); const ref = Banco.#db.collection(colecao); if (id) { await ref.doc(id).set(dados, { merge: true }); return id; } const docRef = await ref.add(dados); return docRef.id; }\n" +
"}\nmodule.exports = Banco;\n";
        }
    }
}
