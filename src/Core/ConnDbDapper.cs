using Microsoft.Extensions.Configuration;
using Npgsql;
using Dapper;

namespace Core
{
    public class ConnDbDapper
    {
        private readonly IConfiguration _config;

        public NpgsqlConnection Conn { get; private set; }

        public ConnDbDapper(IConfiguration config)
        {
            this._config = config;
        }

        public NpgsqlConnection GetConn()
        {
            //TODO : Ajustar para recueprar dados da cionexão default da aplicação
            string defaultConn = _config.GetSection("ConnectionStrings").GetSection("Default").Value;
            this.Conn = new NpgsqlConnection(_config.GetSection("ConnectionStrings").GetSection($"{defaultConn}").Value);
            return this.Conn;
        }

        public NpgsqlConnection OpenConn()
        {
            this.GetConn();

            if (this.Conn == null)
            {
                this.Conn.Open();
            }
            if (this.Conn?.State == System.Data.ConnectionState.Closed)
            {
                this.Conn.Open();
            }

            return this.Conn;
        }

        public void CloseConn()
        {

            if (this.Conn != null)
            {
                if (this.Conn?.State != System.Data.ConnectionState.Closed)
                {
                    this.Conn.Close();
                }
            }

            this.Conn.Dispose();
        }
    }
}
