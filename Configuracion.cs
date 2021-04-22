using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;

namespace PesajeWPF {
    public class Configuracion {
        public static string PuertoCOM {
            get { return Properties.Settings.Default.Puerto; }
        }
        private static SqlConnection _cnn = null;
        public static SqlConnection ConexionBBDD {
            get {
                if (_cnn == null) {
                    _cnn = new SqlConnection(Properties.Settings.Default.ConexionBBDD);
                }
                if (_cnn.State == System.Data.ConnectionState.Closed) _cnn.Open();
                return _cnn;
            }
        }
        public static string NombreEquipo { get { return Dns.GetHostName(); } }
        public static string IPEquipo {
            get {
                string localIP = string.Empty;
                try {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                        socket.Connect("8.8.8.8", 65530);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        localIP = endPoint.Address.ToString();
                    }
                } catch { }
                if (string.IsNullOrEmpty(localIP)) localIP = "127.0.0.1";
                return localIP;
            }
        }
        public static string DirectorioEtiquetas { get { return Properties.Settings.Default.DirectorioEtiquetas; } }

        public static decimal PesoMinimoPesada { get { return Properties.Settings.Default.PesoMinimoPesada; } }
    }
}
