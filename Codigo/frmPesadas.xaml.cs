using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using PesajeWPF.Datos;
using System.IO.Ports;
using Bartender = Seagull.BarTender.Print;
using System.Windows.Threading;
using System.Windows.Controls;
using System.ComponentModel;

namespace PesajeWPF {
    /// <summary>
    /// Lógica de interacción para frmPesadas.xaml
    /// </summary>
    public partial class frmPesadas : Window {
        /// <summary>
        /// oPesos contiene los productos que se están pesando en este momento
        /// </summary>
        private Productos oPesos = new Productos();
        /// <summary>
        /// oPesosProductos contiene todos los productos de todas las cajas del trabajo
        /// </summary>
        private Productos oPesosProductos = new Productos();
        private Cajas oPesosCajas = new Cajas();
        private Trabajo TrabajoActivo = null;
        private bool EstoyPesandoCajas = false;
        private bool EsperandoNuevaPesada = false;

        private SerialPort PuertoCOM = new SerialPort(Configuracion.PuertoCOM);
        private string bufferCOM = string.Empty;
        private System.Windows.Threading.DispatcherTimer Temporizador = new System.Windows.Threading.DispatcherTimer();

        private Bartender.Engine BartenderEngine = null;
        private Bartender.LabelFormatDocument EtiquetaCaja = null;
        private Bartender.LabelFormatDocument EtiquetaProducto = null;
        private Bartender.LabelFormatDocument EtiquetaTotal = null;

        private BackgroundWorker ImprimeEtiquetaWorker = new BackgroundWorker();
        public frmPesadas() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ImprimeEtiquetaWorker.WorkerReportsProgress = true;
            ImprimeEtiquetaWorker.DoWork += new DoWorkEventHandler(ImprimeEtiquetaWorker_DoWork);
            ImprimeEtiquetaWorker.ProgressChanged += new ProgressChangedEventHandler(ImprimeEtiquetaWorker_ProgressChanged);

            lvProdutos.ItemsSource = oPesos;
            lvCajas.ItemsSource = oPesosCajas;
            btnCierreManual.IsEnabled = false;
            gridProductos.IsEnabled = false;
            gridCajas.IsEnabled = false;
            btnIniciar.Tag = null;

            // inicia el motor de Bartender
            try {
                BartenderEngine = new Bartender.Engine(true);
            } catch (Bartender.PrintEngineException ex) {
                Console.WriteLine(ex.Message);
                MessageBox.Show(this, ex.Message, "PesajeWPF");
            }

            try
            {
                PuertoCOM.Handshake = Handshake.None;
                PuertoCOM.Parity = Parity.None;
                PuertoCOM.DataBits = 8;
                PuertoCOM.StopBits = StopBits.One;
                PuertoCOM.ReadTimeout = 200;
                PuertoCOM.WriteTimeout = 50;
                PuertoCOM.ReceivedBytesThreshold = 10;
                PuertoCOM.Open();
                PuertoCOM.DataReceived += PuertoCOM_DataReceived;

                if (PuertoCOM.IsOpen)
                {
                    lblCOM.Content = Configuracion.PuertoCOM;
                    lblCOM.Foreground = Brushes.Black;
                    COMBorder.Background = Brushes.Green;
                    TrabajoActivo = TrabajoGestor.ObtenerTrabajoActivo(Configuracion.NombreEquipo);
                    CargaEtiquetas();
                }
                else
                {
                    //lblCOM.Content = $"Puerto {Configuracion.PuertoCOM} no encontrado";
                    COMBorder.Background = Brushes.Red;
                    lblCOM.Foreground = Brushes.Red;
                    btnIniciar.IsEnabled = false;
                    btnFinalizar.IsEnabled = false;
                }

                if (TrabajoActivo != null)
                {
                    btnIniciar.Content = "INICIADO";
                    btnIniciar.Background = Brushes.Salmon;
                    CargaEtiquetas();
                    CargaCajasActivas(TrabajoActivo.Id);
                    CargaProductosActivos(TrabajoActivo.Id);
                }
                else
                {
                    TrabajoActivo = new Trabajo();
                }
                CargaDatos();

                Temporizador.Tick += new EventHandler(Temporizador_Tick);
                //Temporizador.Interval = new TimeSpan(0, 0, 1);
                Temporizador.Interval = TimeSpan.FromMilliseconds(100);
                Temporizador.Start();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                lblCOM.Content = ex.Message;
            }

        }
        private void ImprimeEtiquetaWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            List<string> lstMensajes = new List<string>();
            if (e.UserState.GetType() == typeof(List<string>)) {
                lstMensajes = (List<string>)e.UserState;
            }

            if (e.ProgressPercentage == 100 && lstMensajes.Count > 0) {
                string msg = "Error al imprimir:" + Environment.NewLine + string.Join(Environment.NewLine, lstMensajes);
                MessageBox.Show(msg, "Error Bartender", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ImprimeEtiquetaWorker_DoWork(object sender, DoWorkEventArgs e) {

            List<string> lstMensajes = new List<string>();
            Bartender.Messages messages = null;

            if (e.Argument != null) {
                // A la función se le puede enviar una sola etiqueta o un listado. Comprobar el tipo del argumento
                List<Bartender.LabelFormatDocument> lstEtiquetas = new List<Bartender.LabelFormatDocument>();
                if (e.Argument.GetType() == typeof(Bartender.LabelFormatDocument)) {
                    lstEtiquetas.Add((Bartender.LabelFormatDocument)e.Argument);
                } else {
                    lstEtiquetas = (List<Bartender.LabelFormatDocument>)e.Argument;
                }

                lock (BartenderEngine) {
                    foreach (Bartender.LabelFormatDocument Etiqueta in lstEtiquetas) {
                        Bartender.Result result = Etiqueta.Print("PesajeWpf", out messages);
                        string messagesString = "Mensajes: ";
                        foreach (Bartender.Message msg in messages) {
                            messagesString += "\n" + msg.Text;
                        }
                        if (result == Bartender.Result.Failure) {
                            // Error en la impresión
                            lstMensajes.AddRange(messages.Select(m => m.Text).ToList());
                        }
                    }
                }
            }

            ImprimeEtiquetaWorker.ReportProgress(100, lstMensajes);
        }

        private void CargaProductosActivos(int idTrabajo) {
            Productos lstProductos = ProductoGestor.ObtenerProductosTrabajo(idTrabajo);
            foreach (Producto producto in lstProductos) {
                if (producto != null) {
                    oPesosProductos.Add(producto);
                    if (producto.IdCaja == 0) {
                        oPesos.Add(producto);
                    }
                }
            }
            if(lstProductos.Count>0)
            {
                lvProdutos.ScrollIntoView(lstProductos.Last());
            }
        }

        private void CargaCajasActivas(int idTrabajo) {
            Cajas lstCajas = CajaGestor.ObtenerCajas(idTrabajo);
            foreach (Caja caja in lstCajas) {
                if (caja != null) {
                    oPesosCajas.Add(caja);
                }
            }
            if (lstCajas.Count > 0)
            {
                lvCajas.ScrollIntoView(lstCajas.Last());
            }
        }

        decimal peso = decimal.Zero;
        string strpeso = decimal.Zero.ToString("N3");
        private void PuertoCOM_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            var serialDevice = sender as SerialPort;
            // obtener el peso del buffer
            //var buffer = new byte[serialDevice.BytesToRead];
            //serialDevice.Read(buffer, 0, buffer.Length);
            bufferCOM += serialDevice.ReadExisting();
            if (bufferCOM.Length < 10) {
                return;
            }
            bool pesadavalida = false;
            List<string> lstPesos = bufferCOM.Replace(".", ",").Split(new string[] { "\r" }, StringSplitOptions.None).ToList();
            foreach (string p in lstPesos)
            {
                string pp = p.Replace(((char)0x02).ToString(), "").Replace("\u0002", "").Replace("\x02", "");
                if (!string.IsNullOrEmpty(p) && (pp.StartsWith("A") || pp.StartsWith("B") || pp.StartsWith("I")) && pp.Length > 8)
                {
                    pesadavalida = true;
                    strpeso = pp.Replace("A", "").Replace("B", "").Replace("I", "").Replace(".", ",").Trim();
                    decimal.TryParse(strpeso, out peso);
                    //if (peso > 0)
                    if (peso > Configuracion.PesoMinimoPesada)
                    {
                        bufferCOM = string.Empty;
                    }
                }
            }

            //string pesoBuffer = bufferCOM.ToString().Replace(".", ",").Replace("\x02", "");
            //pesoBuffer = pesoBuffer.Remove(0, 5).Replace("\r", "");
            //if (!string.IsNullOrEmpty(pesoBuffer) && (pesoBuffer.StartsWith("A") || pesoBuffer.StartsWith("B") || pesoBuffer.StartsWith("I")))
            //{
            //    pesadavalida = true;
            //    decimal.TryParse(pesoBuffer, out peso);
            //    strpeso = pesoBuffer;
            //}

            if (!pesadavalida) return;
            bufferCOM = string.Empty;
            // Controlar peso mayor que peso mínimo (-a cero-)
            if (peso > Configuracion.PesoMinimoPesada && EsperandoNuevaPesada) {
                EsperandoNuevaPesada = false;
                // process data on the GUI thread
                Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    List<Bartender.LabelFormatDocument> lstEtiquetas = new List<Bartender.LabelFormatDocument>();
                    if (TrabajoActivo != null && TrabajoActivo.Id > 0) {
                        if (EstoyPesandoCajas) {
                            #region Genera Caja y la imprime
                            Caja oCaja = new Caja() { Fecha = TrabajoActivo.Fecha, IdTrabajo = TrabajoActivo.Id, Lote = TrabajoActivo.Lote, Peso = peso };
                            int idcaja = CajaGestor.GuardarCaja(oCaja);
                            if (idcaja > 0) {
                                oCaja.Id = idcaja;
                                oCaja.Numero = oPesosCajas.Count + 1;
                                oPesosCajas.Add(oCaja);
                                lvCajas.ScrollIntoView(oPesosCajas.Last());
                                lblCajas.Content = oPesosCajas.Count.ToString("N0");
                                foreach (var x in EtiquetaCaja.SubStrings) {
                                    switch (x.Name.ToLower()) {
                                        case "pesaje_peso": x.Value = oCaja.Peso.ToString(); break;
                                        case "pesaje_lote": x.Value = TrabajoActivo.Lote; break;
                                        case "pesaje_fecha": x.Value = TrabajoActivo.Fecha.ToShortDateString(); break;
                                        case "pesaje_numero": x.Value = oCaja.Numero.ToString(); break;
                                        case "pesaje_numeroproductos": x.Value = oPesosProductos.Where(c => c.IdCaja == oCaja.Id).Count().ToString(); break;
                                        default: break;
                                    }
                                }
                                //ImprimeEtiquetaWorker.RunWorkerAsync(EtiquetaCaja);
                                lstEtiquetas.Add(EtiquetaCaja);
                            }
                            #endregion
                        } else {
                            int idcaja = 0;
                            bool limpiarPesosCaja = false;
                            Producto oProducto = new Producto() { Fecha = TrabajoActivo.Fecha, IdTrabajo = TrabajoActivo.Id, Lote = TrabajoActivo.Lote, Peso = peso };
                            oProducto.Numero = oPesosProductos.Count + 1;
                            #region Genera Caja y laImprime
                            if (TrabajoActivo.NumeroProductosCierre > 0 && oPesos.Count + 1 == TrabajoActivo.NumeroProductosCierre) {
                                // Generar caja
                                limpiarPesosCaja = true;
                                Caja oCaja = new Caja() { Fecha = TrabajoActivo.Fecha, IdTrabajo = TrabajoActivo.Id, Lote = TrabajoActivo.Lote, Peso = peso };
                                oCaja.Peso = oPesos.Sum(p => p.Peso) + peso;
                                oCaja.Numero = oPesosCajas.Count + 1;
                                idcaja = CajaGestor.GuardarCaja(oCaja);
                                if (idcaja > 0) {
                                    oCaja.Id = idcaja;
                                    oPesosCajas.Add(oCaja);
                                    lvCajas.ScrollIntoView(oPesosCajas.Last());
                                    foreach (var x in EtiquetaCaja.SubStrings) {
                                        switch (x.Name.ToLower()) {
                                            case "pesaje_peso": x.Value = oCaja.Peso.ToString(); break;
                                            case "pesaje_lote": x.Value = TrabajoActivo.Lote; break;
                                            case "pesaje_fecha": x.Value = TrabajoActivo.Fecha.ToShortDateString(); break;
                                            case "pesaje_numero": x.Value = oCaja.Numero.ToString(); break;
                                            case "pesaje_numeroproductos": x.Value = (oPesosProductos.Where(c => c.IdCaja == oCaja.Id).Count() + 1).ToString(); break;
                                            default: break;
                                        }
                                    }
                                    //ImprimeEtiquetaWorker.RunWorkerAsync(EtiquetaCaja);
                                    lstEtiquetas.Add(EtiquetaCaja);
                                    lblCajas.Content = oPesosCajas.Count.ToString("N0");
                                }
                            }
                            #endregion
                            #region Genera el Producto y lo imprime
                            oProducto.IdCaja = idcaja;  //¿Esto sigue haciendo falta con el nuevo método?
                            oProducto.Numero = oPesosProductos.Count + 1;
                            // Guardar peso del producto y controlar el número de productos que finalizan una caja
                            int idproducto = ProductoGestor.GuardaProducto(oProducto);
                            if (idproducto > 0) {
                                oProducto.Id = idproducto;
                                oPesosProductos.Add(oProducto);
                                oPesos.Add(oProducto);
                                lvProdutos.ScrollIntoView(oPesos.Last());
                                foreach (var x in EtiquetaProducto.SubStrings) {
                                    switch (x.Name.ToLower()) {
                                        case "pesaje_peso": x.Value = oProducto.Peso.ToString(); break;
                                        case "pesaje_lote": x.Value = TrabajoActivo.Lote; break;
                                        case "pesaje_fecha": x.Value = TrabajoActivo.Fecha.ToShortDateString(); break;
                                        case "pesaje_numero": x.Value = oProducto.Numero.ToString(); break;
                                        default: break;
                                    }
                                }
                                //ImprimeEtiquetaWorker.RunWorkerAsync(EtiquetaProducto);
                                lstEtiquetas.Insert(0, EtiquetaProducto);

                                if (limpiarPesosCaja) {
                                    foreach(Producto p in oPesos) {
                                        ProductoGestor.AsignaCajaProducto(p.Id, idcaja);
                                    }
                                    oPesos.Clear();
                                }
                                lblProductos.Content = oPesosProductos.Count.ToString("N0");
                            }
                            #endregion
                        }
                        if (lstEtiquetas.Count > 0) {
                            ImprimeEtiquetaWorker.RunWorkerAsync(lstEtiquetas);
                        }
                        // Mostrar el peso de las cajas/productos
                        if (oPesosCajas.Count == 0) {
                            lblKilos.Content = oPesosProductos.Sum(p => p.Peso).ToString();
                        } else {
                            lblKilos.Content = oPesosCajas.Sum(p => p.Peso).ToString("N3");
                        }
                    }

                }));
            } else {
                //if (peso == 0) {
                if (peso < Configuracion.PesoMinimoPesada) { 
                    //Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                    //    lblPeso.Content = peso.ToString("N3");
                    //}));
                    EsperandoNuevaPesada = true;
                }
            }
            EnviarElDolar = true;
        }
        bool EnviarElDolar = true;
        private void Temporizador_Tick(object sender, EventArgs e) {
            EnviarElDolar = true;
            // enviar al puerto COM la petición del peso
            if (PuertoCOM.IsOpen && EnviarElDolar) {
                PuertoCOM.Write("$");
                EnviarElDolar = false;
            }
            lblPeso.Content = strpeso + "  [" + DateTime.Now.ToString("HH:mm:ss") + "]";
        }

        private void CargaEtiquetas() {
            if (TrabajoActivo != null && TrabajoActivo.Id > 0) {
                /* Etiqueta de Producto */
                // Comprobar si existe el archivo 
                lblMensajes.Text = string.Empty;
                if (!string.IsNullOrEmpty(TrabajoActivo.EtiquetaProducto) && System.IO.File.Exists(TrabajoActivo.EtiquetaProducto)) {
                    // Cargar la etiqueta sólo si no hay ninguna cargada o el archivo es distinto al cargado
                    if (EtiquetaProducto == null || (EtiquetaProducto != null && EtiquetaProducto.FileName != TrabajoActivo.EtiquetaProducto)) {
                        // Cerrar la etiqueta si hay alguna
                        if (EtiquetaProducto != null) EtiquetaProducto.Close(Bartender.SaveOptions.DoNotSaveChanges);
                        EtiquetaProducto = BartenderEngine.Documents.Open(TrabajoActivo.EtiquetaProducto);
                    }
                    EtiquetaProducto.PrintSetup.PrinterName = TrabajoActivo.ImpresoraProducto;
                    if (EtiquetaProducto.PrintSetup.SupportsIdenticalCopies) EtiquetaProducto.PrintSetup.IdenticalCopiesOfLabel = TrabajoActivo.CopiasProducto;
                    //
                    foreach (var x in EtiquetaProducto.SubStrings) {
                        switch (x.Name.ToLower()) {
                            case "pesaje_peso": x.Value = "0"; break;
                            case "pesaje_lote": x.Value = string.Empty; break;
                            case "pesaje_fecha": x.Value = DateTime.Today.ToShortDateString(); break;
                            case "pesaje_numero": x.Value = "0"; break;
                            default: break;
                        }
                    }
                } else {
                    EtiquetaProducto = null;
                    lblMensajes.Text += Environment.NewLine + "Etiqueta de producto no existe";
                }
                /* Etiqueta de Caja */
                if (!string.IsNullOrEmpty(TrabajoActivo.EtiquetaCaja) && System.IO.File.Exists(TrabajoActivo.EtiquetaCaja)) {
                    if (EtiquetaCaja == null || (EtiquetaCaja != null && EtiquetaCaja.FileName != TrabajoActivo.EtiquetaCaja)) {
                        if (EtiquetaCaja != null) EtiquetaCaja.Close(Bartender.SaveOptions.DoNotSaveChanges);
                        EtiquetaCaja = BartenderEngine.Documents.Open(TrabajoActivo.EtiquetaCaja);
                    }
                    EtiquetaCaja.PrintSetup.PrinterName = TrabajoActivo.ImpresoraCaja;
                    if (EtiquetaCaja.PrintSetup.SupportsIdenticalCopies) EtiquetaCaja.PrintSetup.IdenticalCopiesOfLabel = TrabajoActivo.CopiasCaja;
                    //
                    foreach (var x in EtiquetaCaja.SubStrings) {
                        switch (x.Name.ToLower()) {
                            case "pesaje_peso": x.Value = "0"; break;
                            case "pesaje_lote": x.Value = string.Empty; break;
                            case "pesaje_fecha": x.Value = DateTime.Today.ToShortDateString(); break;
                            case "pesaje_numero": x.Value = "0"; break;
                            case "pesaje_numeroproductos": x.Value = "0"; break;
                            default: break;
                        }
                    }
                } else {
                    EtiquetaCaja = null;
                    lblMensajes.Text += Environment.NewLine + "Etiqueta de caja no existe";
                }
                /* Etiqueta de Total  */
                if (!string.IsNullOrEmpty(TrabajoActivo.EtiquetaTotal) && System.IO.File.Exists(TrabajoActivo.EtiquetaTotal)) {
                    if (EtiquetaTotal == null || (EtiquetaTotal != null && EtiquetaTotal.FileName != TrabajoActivo.EtiquetaTotal)) {
                        if (EtiquetaTotal != null) EtiquetaTotal.Close(Bartender.SaveOptions.DoNotSaveChanges);
                        EtiquetaTotal = BartenderEngine.Documents.Open(TrabajoActivo.EtiquetaTotal);
                    }
                    EtiquetaTotal.PrintSetup.PrinterName = TrabajoActivo.ImpresoraTotal;
                    if (EtiquetaTotal.PrintSetup.SupportsIdenticalCopies) EtiquetaTotal.PrintSetup.IdenticalCopiesOfLabel = TrabajoActivo.CopiasTotal;
                    //
                    foreach (var x in EtiquetaTotal.SubStrings) {
                        switch (x.Name.ToLower()) {
                            case "pesaje_peso": x.Value = "0"; break;
                            case "pesaje_lote": x.Value = string.Empty; break;
                            case "pesaje_fecha": x.Value = DateTime.Today.ToShortDateString(); break;
                            case "pesaje_numero": x.Value = "0"; break;
                            case "pesaje_numerocajas": x.Value = "0"; break;
                            default: break;
                        }
                    }
                } else {
                    EtiquetaTotal = null;
                    lblMensajes.Text += Environment.NewLine + "Etiqueta de total no existe";
                }
                if (!string.IsNullOrEmpty(lblMensajes.Text.ToString())) {
                    lblMensajes.Text = "ETIQUETAS:" + lblMensajes.Text;
                }
            } else {
                EtiquetaCaja = null;
                EtiquetaProducto = null;
                EtiquetaTotal = null;
            }
        }
        private void CargaDatos() {
            gridProductos.IsEnabled = !string.IsNullOrEmpty(TrabajoActivo.EtiquetaProducto);
            gridCajas.IsEnabled = !string.IsNullOrEmpty(TrabajoActivo.EtiquetaCaja) || gridProductos.IsEnabled;
            btnIniciar.Tag = TrabajoActivo;

            lblEtiquetaProducto.Content = System.IO.Path.GetFileName(TrabajoActivo.EtiquetaProducto);
            lblEtiquetaProducto.Foreground = Brushes.Black;
            if (!System.IO.File.Exists(TrabajoActivo.EtiquetaProducto)) lblEtiquetaProducto.Foreground = Brushes.Red;

            lblEtiquetaCaja.Content = System.IO.Path.GetFileName(TrabajoActivo.EtiquetaCaja);
            lblEtiquetaCaja.Foreground = Brushes.Black;
            if (!System.IO.File.Exists(TrabajoActivo.EtiquetaCaja))
            {
                lblEtiquetaCaja.Foreground = Brushes.Red;
                SwitchControlesPeso();
            }

            lblImpresoraProducto.Content = System.IO.Path.GetFileName(TrabajoActivo.ImpresoraProducto);
            lblImpresoraCaja.Content = TrabajoActivo.ImpresoraCaja;
            lblCopiasProducto.Content = TrabajoActivo.CopiasProducto;
            lblCopiasCaja.Content = TrabajoActivo.CopiasCaja;
            //lblCajaAutoEn.Content = TrabajoActivo.NumeroProductosCierre;
            //btnCierreManual.IsEnabled = gridProductos.IsEnabled;
            SwitchControlesPeso();

            EstoyPesandoCajas = !gridProductos.IsEnabled;

            lblLote.Content = TrabajoActivo.Lote;
            lblFecha.Content = TrabajoActivo.Fecha.ToShortDateString();

            ActualizaLabels();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (Temporizador != null) Temporizador.Stop();
            // Preguntar si quiere cerrar.
            if (bPedirConfirmacion) {
                if (MessageBox.Show("¿Salir del programa?", "Cerrar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) {
                    e.Cancel = true;
                }
            }
            if (!e.Cancel) {
                if (PuertoCOM.IsOpen) PuertoCOM.Close();
                if (BartenderEngine != null) BartenderEngine.Stop(Bartender.SaveOptions.DoNotSaveChanges);
            } else {
                if (Temporizador != null) Temporizador.Start();

            }
        }
        private void ActualizaLabels()
        {
            //lblProductos y lblKilos
            if (oPesosProductos.Count == 0) {
                lblProductos.Content = 0;
            } else {
                lblProductos.Content = oPesosProductos.Count.ToString("N0");
            }

            //lblCajas
            if (oPesosCajas.Count == 0) {
                lblCajas.Content = 0;
            }
            else {
                lblCajas.Content = oPesosCajas.Count.ToString("N0");
            }
            //lblKilos
            // Suma los pesos de las cajas y también los productos que están pendientes de meter en una caja
            decimal totalkg = decimal.Zero;
            if (oPesosCajas.Count > 0) totalkg += oPesosCajas.Sum(p => p.Peso);
            if (oPesos.Count > 0) totalkg += oPesos.Sum(p => p.Peso);
            lblKilos.Content = totalkg.ToString("N3");

        }

        bool bPedirConfirmacion = true;
        /// <summary>
        /// Cierra el programa sin finalizar el trabajo. Se pide confirmación al usuario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCerrar_Click(object sender, RoutedEventArgs e) {
            bPedirConfirmacion = true;
            Close();
        }

        /// <summary>
        /// Cierra el programa finalizando el proceso e imprime la etiqueta de total.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFinalizar_Click(object sender, RoutedEventArgs e) {
            Temporizador.Stop();
            // Finalizar trabajo
            if (TrabajoActivo.Id != 0) {
                if (MessageBox.Show("Finalizar trabajo?", "Finalizar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    if (oPesos.Count == 0 || string.IsNullOrEmpty(TrabajoActivo.EtiquetaCaja)) {
                        // Imprimir etiqueta del total antes de generar nuevo trabajo
                        try
                        {
                            if (MessageBox.Show("Imprimir etiqueta del total?", "Finalizar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                foreach (var x in EtiquetaTotal.SubStrings)
                                {
                                    switch (x.Name.ToLower())
                                    {
                                        case "pesaje_peso": x.Value = oPesosProductos.Sum(p => p.Peso).ToString(); break;
                                        case "pesaje_lote": x.Value = TrabajoActivo.Lote; break;
                                        case "pesaje_fecha": x.Value = TrabajoActivo.Fecha.ToShortDateString(); break;
                                        case "pesaje_numero": x.Value = TrabajoActivo.Id.ToString(); break;
                                        case "pesaje_numerocajas": x.Value = TrabajoActivo.TotalCajas.ToString(); break;
                                        default: break;
                                    }
                                }
                                ImprimeEtiquetaWorker.RunWorkerAsync(EtiquetaTotal);
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Error al cargar la etiqueta de total", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        TrabajoGestor.FinalizarTrabajo(TrabajoActivo.Id);
                        TrabajoActivo = new Trabajo();
                        btnCierreManual.IsEnabled = false;
                        gridProductos.IsEnabled = false;
                        gridCajas.IsEnabled = false;
                        btnIniciar.Tag = null;
                        btnIniciar.Content = "INICIAR";
                        btnIniciar.Background = Brushes.LightGreen;
                        oPesosCajas.Clear();
                        oPesos.Clear();
                        oPesosProductos.Clear();
                        CargaDatos(); 
                    }
                    else {
                        MessageBox.Show("Aún hay productos por pasar a caja", "Error al finalizar", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            Temporizador.Start();
        }

        private void btnListado_Click(object sender, RoutedEventArgs e) {
            Temporizador.Stop();
            // TODO: Mostrar listado de los últimos trabajos por si deseamos reimprimir la etiqueta de todal de un trabajo anterior
            frmTrabajos frm = new frmTrabajos();
            frm.ShowDialog();
            if (frm.DialogResult.HasValue && frm.DialogResult.Value == true) {
                if (frm.TrabajoSeleccionado != null) {
                    if (System.IO.File.Exists(frm.TrabajoSeleccionado.EtiquetaTotal)) {
                        Bartender.LabelFormatDocument ReimprimirEtiquetaTotal = BartenderEngine.Documents.Open(frm.TrabajoSeleccionado.EtiquetaTotal);
                        ReimprimirEtiquetaTotal.PrintSetup.PrinterName = frm.TrabajoSeleccionado.ImpresoraTotal;
                        if (ReimprimirEtiquetaTotal.PrintSetup.SupportsIdenticalCopies) ReimprimirEtiquetaTotal.PrintSetup.IdenticalCopiesOfLabel = frm.TrabajoSeleccionado.CopiasTotal;
                        ImprimeEtiquetaWorker.RunWorkerAsync(ReimprimirEtiquetaTotal);
                        ReimprimirEtiquetaTotal.Close(Bartender.SaveOptions.DoNotSaveChanges);
                    }
                }
            }
            Temporizador.Start();
        }

        private void btnEliminaProducto_Click(object sender, RoutedEventArgs e) {
            Temporizador.Stop();
            if (oPesosProductos.Count > 0) {
                if (MessageBox.Show("¿Eliminar el último producto?", "Eliminar producto", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    Producto oProducto = oPesosProductos.Last();
                    if (ProductoGestor.EliminaProducto(oProducto.Id)) {
                        oPesosProductos.Remove(oProducto);
                        oPesos.Remove(oPesos.Last());
                        ActualizaLabels();
                    }
                }
            }
            Temporizador.Start();
        }

        private void btnReimprimeProducto_Click(object sender, RoutedEventArgs e) {
            Temporizador.Stop();
            if (oPesosProductos.Count > 0) {
                // TODO: Cargar las variables para pasar el peso y los datos a la etiqueta
                Producto pp = oPesosProductos.Last();
                foreach (var x in EtiquetaProducto.SubStrings) {
                    switch (x.Name.ToLower()) {
                        case "pesaje_peso": x.Value = pp.Peso.ToString(); break;
                        case "pesaje_lote": x.Value = TrabajoActivo.Lote; break;
                        case "pesaje_fecha": x.Value = TrabajoActivo.Fecha.ToShortDateString(); break;
                        case "pesaje_numero": x.Value = pp.Numero.ToString(); break;
                        default: break;
                    }
                }
                try
                {
                    ImprimeEtiquetaWorker.RunWorkerAsync(EtiquetaProducto);
                }
                catch (Exception)
                {
                    MessageBox.Show("Espere mientras se completa la impresión", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                };
            }
            Temporizador.Start();
        }

        private void ImprimeEtiqueta(Bartender.LabelFormatDocument Etiqueta) {
            if (Etiqueta != null) {
                lock (BartenderEngine) {
                    Bartender.Messages messages;
                    //int waitForCompletionTimeout = 10000; // 10 segundos
                    Bartender.Result result = Etiqueta.Print("PesajeWpf", out messages);
                    string messagesString = "Mensajes: ";
                    foreach (Bartender.Message msg in messages) {
                        messagesString += "\n" + msg.Text;
                    }
                    if (result == Bartender.Result.Failure) {
                        // Error en la impresión
                        string msg = "Error al imprimir:" + Environment.NewLine + string.Join(Environment.NewLine, messages.Select(m => m.Text).ToList());
                        MessageBox.Show(msg, "Error Bartender", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
        }

        private void btnCierreManual_Click(object sender, RoutedEventArgs e) {
            Temporizador.Stop();
            if (oPesos.Count > 0 && oPesos.Sum(p => p.Peso) > decimal.Zero) {
                if (oPesos.Any()) //Comprueba si hay una pesada
                {
                    if (MessageBox.Show("¿Cerrar la caja?", "Cierre Manual", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                        Caja oCaja = new Caja()
                        {
                            IdTrabajo = TrabajoActivo.Id,
                            Lote = TrabajoActivo.Lote,
                            Fecha = TrabajoActivo.Fecha,
                            Numero = oPesosCajas.Count + 1,
                            Peso = oPesos.Sum(p => p.Peso)
                        };
                        int idcaja = CajaGestor.GuardarCaja(oCaja);
                        if (idcaja > 0)
                        {
                            oCaja.Id = idcaja;
                            oPesosCajas.Add(oCaja);
                            lvCajas.ScrollIntoView(oPesosCajas.Last());
                            oPesos.Clear();
                            // Imprimir la etiqueta de caja

                            //Actualizar EtiquetaCaja
                            foreach (var x in EtiquetaCaja.SubStrings)
                            {
                                switch (x.Name.ToLower())
                                {
                                    case "pesaje_peso": x.Value = oCaja.Peso.ToString(); break;
                                    case "pesaje_lote": x.Value = TrabajoActivo.Lote; break;
                                    case "pesaje_fecha": x.Value = TrabajoActivo.Fecha.ToShortDateString(); break;
                                    case "pesaje_numero": x.Value = oCaja.Numero.ToString(); break;
                                    case "pesaje_numeroproductos": x.Value = oPesosProductos.Where(c => c.IdCaja == oCaja.Id).Count().ToString(); break;
                                    default: break;
                                }
                            }

                            try {
                                ImprimeEtiquetaWorker.RunWorkerAsync(EtiquetaCaja);
                            } catch (Exception) {
                                MessageBox.Show("Espere mientras se completa la impresión", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                            };
                        } 
                    }
                }
            }
            Temporizador.Start();
        }

        private void btnEliminaCaja_Click(object sender, RoutedEventArgs e) {
            Temporizador.Start();
            if (oPesosCajas.Count > 0) {
                if (MessageBox.Show("¿Eliminar la última caja?", "Eliminar caja", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                    Caja oCaja = oPesosCajas.Last();
                    if (CajaGestor.EliminaCaja(oCaja.Id)) {
                        oPesosCajas.Remove(oCaja);
                        ActualizaLabels();
                    }
                }
            }
            Temporizador.Start();
        }

        private void btnReimprimeCaja_Click(object sender, RoutedEventArgs e) {
            Temporizador.Stop();
            if (oPesosCajas.Count > 0) {
                // TODO: Cargar las variables para pasar el peso y los datos a la etiqueta
                Caja cc = oPesosCajas.Last();
                foreach (var x in EtiquetaCaja.SubStrings) {
                    switch (x.Name.ToLower()) {
                        case "pesaje_peso": x.Value = cc.Peso.ToString(); break;
                        case "pesaje_lote": x.Value = TrabajoActivo.Lote; break;
                        case "pesaje_fecha": x.Value = TrabajoActivo.Fecha.ToShortDateString(); break;
                        case "pesaje_numero": x.Value = cc.Numero.ToString(); break;
                        case "pesaje_numeroproductos": x.Value = oPesosProductos.Where(c => c.IdCaja == cc.Id).Count().ToString(); break;
                        default: break;
                    }
                }
                try {
                    ImprimeEtiquetaWorker.RunWorkerAsync(EtiquetaCaja);
                } catch (Exception) {
                    MessageBox.Show("Espere mientras se completa la impresión", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                };
            }
            Temporizador.Start();
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e) {
            Temporizador.Stop();
            frmConfiguracionTrabajo frm = new frmConfiguracionTrabajo();
            frm.ShowDialog();
            if (frm.DialogResult.HasValue && frm.DialogResult.Value == true) {
                TrabajoActivo = TrabajoGestor.ObtenerTrabajoActivo(Configuracion.NombreEquipo);
                CargaEtiquetas();
                btnIniciar.Content = "INICIADO";
                btnIniciar.Background = Brushes.Salmon;
                CargaDatos();
            }
            Temporizador.Start();
        }

        
        public void SwitchControlesPeso() {
            //Activa o desactiva los controles de cierre manual y caja automática
            if(string.IsNullOrEmpty(TrabajoActivo.EtiquetaCaja) || string.IsNullOrEmpty(TrabajoActivo.EtiquetaProducto)) {
                lblCajaAutoEn.IsEnabled = false;
                lblCajaAutoEn.Content = "-";
                TrabajoActivo.NumeroProductosCierre = 0;
                btnCierreManual.IsEnabled = false;
            }
            else {
                lblCajaAutoEn.IsEnabled = true;
                lblCajaAutoEn.Content = TrabajoActivo.NumeroProductosCierre;
                btnCierreManual.IsEnabled = true;
            }
        }
    }
}
