namespace NodePunch.Core
{
    internal enum TipoBanco
    {
        Nenhum,
        MySQL,
        PostgreSQL,
        Firebase
    }

    internal class ConexaoBanco
    {
        public TipoBanco Tipo { get; set; } = TipoBanco.Nenhum;

        // MySQL / PostgreSQL
        public string Server { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Schema { get; set; }
        public string Porta { get; set; }
        public bool ComSP { get; set; }

        // Firebase
        public string FirebaseProjectId { get; set; }
        public string FirebaseServiceAccountPath { get; set; } // caminho do JSON de credenciais

        public ConexaoBanco() { }
    }
}
