using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace AppControl
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SocketServer socket;
        private const int LED_PIN_SEM1 = 21;
        private const int LED_PIN_SEM2 = 20;
        private const int LED_PIN_SEM3 = 16;
        private const int Led_Pin_Loop1 = 26;
        private const int Led_Pin_Loop2 = 7;
        private const int AlarmaSonora_Pin = 18;

        private const int TALANQUERA_PIN_1 = 25;
        private const int TALANQUERA_PIN_2 = 12;
        private const int TALANQUERA_PIN_3 = 24;

        private GpioPin pinSem1;
        private GpioPin pinSem2;
        private GpioPin pinSem3;
        private GpioPin pinTalanquera1;
        private GpioPin pinTalanquera2;
        private GpioPin pinTalanquera3;
        private GpioPin pinLoop1;
        private GpioPin pinLoop2;
        private GpioPin pinAlarmaSonora;
       // private SerialDevice serialPort = null;
       // private ConexionSerial serialCon;
        private static IAsyncAction workItemThread;
        private bool bonderaTalanquera1 = false;
        private bool cierreTalanquera = false;
        private bool cierreTalanquera2 = false;
        private bool cierreTalanquera3 = false;
        private bool bonderaTalanquera2 = false;
        private bool bonderaTalanquera3 = false;
      //  private int contVehiculosEnCola = 0;

        public  MainPage()
        {
            this.InitializeComponent();
           
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            try
            {

                InitialiseGpio();
                iniciarservidorAsync();
                
                CoreApplication.Exiting += CoreApplication_Exiting;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("\n Ocurrio un error en el MaiPage " + ex.Message);
                txtMensajes.Text += "\n Ocurrio un error en el MaiPage " + ex.Message;
            }
        }

        private void CoreApplication_Exiting(object sender, object e)
        {
            Debug.WriteLine("\n La aplicación se esta cerrando!!!");
            
            RestartHelper(Windows.System.ShutdownKind.Restart);
        }

        private async Task iniciarservidorAsync()
        {
            socket = new SocketServer(9000);
            
            await ThreadPool.RunAsync(x =>
             {
                 socket.OnError += socket_OnError;
                 socket.OnDataRecived += Socket_OnDataRecived;
                 socket.Star();
                 Serial();

             });


            TimeSpan period = TimeSpan.FromMilliseconds(200);

            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        if (socket.status)
                            EstadoPerifericos();
                    });

            }, period);
        }

        private void EstadoPerifericos()
        {
            try
            {
                String estadoDispositivos = "";
                /********************** SEMAFORO 1 *****************************/
                if (pinSem1.Read() == GpioPinValue.Low)
                {
                    estadoDispositivos += "i0;";
                    imgSemaforo1.Source = new BitmapImage(new Uri("ms-appx:///Assets/semaforo-02.png"));
                }
                else
                {
                    estadoDispositivos += "i1;";
                    imgSemaforo1.Source = new BitmapImage(new Uri("ms-appx:///Assets/semaforo-01.png"));
                }
                   
                /********************** SEMAFORO 2*******************************/
                if (pinSem2.Read() == GpioPinValue.Low)
                {
                    estadoDispositivos += "0;";
                    imgSemaforo2.Source = new BitmapImage(new Uri("ms-appx:///Assets/semaforo-02.png"));
                }

                else
                {
                    estadoDispositivos += "1;";
                    imgSemaforo2.Source = new BitmapImage(new Uri("ms-appx:///Assets/semaforo-01.png"));
                }
                    
                /********************** SEMAFORO 3*******************************/
                if (pinSem3.Read() == GpioPinValue.Low)
                {
                    estadoDispositivos += "0;";
                    imgSemaforo3.Source = new BitmapImage(new Uri("ms-appx:///Assets/semaforo-02.png"));
                }
                else
                {
                    estadoDispositivos += "1;";
                    imgSemaforo3.Source = new BitmapImage(new Uri("ms-appx:///Assets/semaforo-01.png"));
                }
                    
                /********************* TALANQUERA 1*****************************/
                if (!bonderaTalanquera1)
                {                    
                    estadoDispositivos += "0;";
                    imgTalanquera1.Source = new BitmapImage(new Uri("ms-appx:///Assets/talanquera-01.png"));
                }
                else
                {
  
                    estadoDispositivos += "1;";
                    imgTalanquera1.Source = new BitmapImage(new Uri("ms-appx:///Assets/talanquera-02.png"));
                }
                    
                /********************* TALANQUERA 2*****************************/
                if (!bonderaTalanquera2)
                { 
                    estadoDispositivos += "0;";
                    imgTalanquera2.Source = new BitmapImage(new Uri("ms-appx:///Assets/talanquera-01.png"));
                }
                else
                {
                    estadoDispositivos += "1;";
                    imgTalanquera2.Source = new BitmapImage(new Uri("ms-appx:///Assets/talanquera-02.png"));
                }
                    
                /********************* TALANQUERA 3*****************************/
                if (!bonderaTalanquera3)
                {
                    estadoDispositivos += "0;";
                    imgTalanquera3.Source = new BitmapImage(new Uri("ms-appx:///Assets/talanquera-01.png"));
                }
                else
                {
                    estadoDispositivos += "1;";
                    imgTalanquera3.Source = new BitmapImage(new Uri("ms-appx:///Assets/talanquera-02.png"));
                }
                    
                /********************* LOOP 1 *****************************/
                if (pinLoop1.Read() == GpioPinValue.High)
                {
                    estadoDispositivos += "0;";
                    imgLoop1.Source = new BitmapImage(new Uri("ms-appx:///Assets/activo.png"));
                }

                else
                {
                    estadoDispositivos += "1;";
                    imgLoop1.Source = new BitmapImage(new Uri("ms-appx:///Assets/inactivo.png"));
                }
                    
                /********************* LOOP 2 *****************************/
                if (pinLoop2.Read() == GpioPinValue.High)
                {
                    estadoDispositivos += "0";
                    imgLoop2.Source = new BitmapImage(new Uri("ms-appx:///Assets/activo.png"));
                }
                else
                {
                    estadoDispositivos += "1";
                    imgLoop2.Source = new BitmapImage(new Uri("ms-appx:///Assets/inactivo.png"));
                }
                /********************* ALARMA SONORA *******************************/
                if(pinAlarmaSonora.Read()==GpioPinValue.Low)
                {
                    estadoDispositivos += "0";
                }
                else
                {
                    estadoDispositivos += "1;";
                }

                /**************************************************/
                //txtMensajes.Text += "\n Estado Perifericos" + estadoDispositivos;
              //  Debug.WriteLine("DATOS ENVIADOS="+ estadoDispositivos);
                socket.Send(estadoDispositivos);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ocurrio una excepción con EstadoPerifericos " + ex);
            }


        }


        private void InitialiseGpio()
        {
            GpioController controller = GpioController.GetDefault();

            //Muestra un error si no hay un controlador GPIO
            if (controller != null)
            {

                //Se Configura el pin del semaforo 1 led rojo como salida
                pinSem1 = controller.OpenPin(LED_PIN_SEM1);
                pinSem1.Write(GpioPinValue.Low);
                pinSem1.SetDriveMode(GpioPinDriveMode.Output);

                //Se Configura el pin del semaforo 2 led verde como salida
                pinSem2 = controller.OpenPin(LED_PIN_SEM2);
                pinSem2.Write(GpioPinValue.Low);
                pinSem2.SetDriveMode(GpioPinDriveMode.Output);
                //Se Configura el pin del semaforo 3 led verde como salida
                pinSem3 = controller.OpenPin(LED_PIN_SEM3);
                pinSem3.Write(GpioPinValue.Low);
                pinSem3.SetDriveMode(GpioPinDriveMode.Output);

                //configuro las talanqueras salida
                pinTalanquera1 = controller.OpenPin(TALANQUERA_PIN_1);
                pinTalanquera1.Write(GpioPinValue.Low);
                pinTalanquera1.SetDriveMode(GpioPinDriveMode.Output);

                pinTalanquera2 = controller.OpenPin(TALANQUERA_PIN_2);
                pinTalanquera2.Write(GpioPinValue.Low);
                pinTalanquera2.SetDriveMode(GpioPinDriveMode.Output);

                pinTalanquera3 = controller.OpenPin(TALANQUERA_PIN_3);
                pinTalanquera3.Write(GpioPinValue.Low);
                pinTalanquera3.SetDriveMode(GpioPinDriveMode.Output);

                //configuramos los LOOP datos de entrada
                pinLoop1 = controller.OpenPin(Led_Pin_Loop1);
                pinLoop2 = controller.OpenPin(Led_Pin_Loop2);

                //Configuración de Alarma sonora como salida 
                pinAlarmaSonora = controller.OpenPin(AlarmaSonora_Pin);
                pinAlarmaSonora.Write(GpioPinValue.Low);
                pinAlarmaSonora.SetDriveMode(GpioPinDriveMode.Output);

                //Se configuran los loop como datos de entrada y se generan los eventos
                if (pinLoop1.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                    pinLoop1.SetDriveMode(GpioPinDriveMode.InputPullUp);
                else
                    pinLoop1.SetDriveMode(GpioPinDriveMode.Input);

                //Se establece un tiempo de espera para la eliminación de rebote y ruido al momento de presionar el boton
                pinLoop1.DebounceTimeout = TimeSpan.FromMilliseconds(100);

                if (pinLoop2.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                    pinLoop2.SetDriveMode(GpioPinDriveMode.InputPullUp);
                else
                    pinLoop2.SetDriveMode(GpioPinDriveMode.Input);

                pinLoop2.DebounceTimeout = TimeSpan.FromMilliseconds(100);



                //*Se establece un evento que se ejecuta cada ves que el boton cambia de estado
                pinLoop1.ValueChanged += pinLoop1_ValueChanged;
                pinLoop2.ValueChanged += pinLoop2_ValueChanged;

               // verificacionPerifericos();



                Debug.WriteLine("Pines GPIO inicializados correctamente");
                txtMensajes.Text += "Pines GPIO inicializados correctamente";

            }
            else
            {
                Debug.WriteLine("No hay controlador GPIO en este dispositivo");
                txtMensajes.Text += "No hay controlador GPIO en este dispositivo";
            }
        }

        private void pinLoop1_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            //try
            //{
            //        if (!bonderaTalanquera2)
            //        {
            //            bonderaTalanquera2 = false;
            //            pinTalanquera2.Write(GpioPinValue.High);
            //            Task.Delay(200).Wait();
            //            pinTalanquera2.Write(GpioPinValue.Low);
            //            pinSem2.Write(GpioPinValue.Low);
            //        }                

            //}
            //catch (Exception ex)
            //{

            //    Debug.WriteLine("Ocurrio una excepción con el evento boton" + ex);
            //}
        }

        private void pinLoop2_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {

           // Debug.WriteLine("TOTAL VEHICULOS EN COLA= " + contVehiculosEnCola);
            try
            {

                //if (cierreTalanquera3)
                //{
                //    if (args.Edge != GpioPinEdge.FallingEdge)
                //    {
                //        bonderaTalanquera3 = false;
                //        pinTalanquera3.Write(GpioPinValue.High);
                //        Task.Delay(200).Wait();
                //        pinTalanquera3.Write(GpioPinValue.Low);
                //        pinSem3.Write(GpioPinValue.Low);
                //        cierreTalanquera3 = false;
                //    }
                //}

                if (args.Edge != GpioPinEdge.FallingEdge)
                {
                    pinAlarmaSonora.Write(GpioPinValue.Low);
                }


            }
            catch (Exception ex)
            {

                Debug.WriteLine("Ocurrio una excepción con el evento boton" + ex);
                txtMensajes.Text += "\n Ocurrio una excepción con el evento boton" + ex;
            }
        }


        private void Socket_OnDataRecived(string data)
        {
            // txtMensajes.Text += "\n Mensaje Recibido=" + data;
            // String estadoDispositivos = "";
            String[] perifericos = data.Split(';');



                if (perifericos.Count() == 3)
                {
                    if (perifericos[0] == "1")
                    {
                        pinTalanquera1.Write(GpioPinValue.High);
                        Task.Delay(200).Wait();
                        pinTalanquera1.Write(GpioPinValue.Low);
                    }
                    if (perifericos[1] == "1")
                    {
                        pinTalanquera2.Write(GpioPinValue.High);
                        Task.Delay(200).Wait();
                        pinTalanquera2.Write(GpioPinValue.Low);
                    }
                    if (perifericos[2] == "1")
                    {
                        pinTalanquera3.Write(GpioPinValue.High);
                        Task.Delay(200).Wait();
                        pinTalanquera3.Write(GpioPinValue.Low);
                    }
                    return;

            }
            //Manejo del led semaforo 1
            if (perifericos[0] == "1")
                pinSem1.Write(GpioPinValue.High);
            else
                pinSem1.Write(GpioPinValue.Low);
            //Manejo del semaforo 2
            if (perifericos[1] == "1")
             {               
                if (!bonderaTalanquera1)
                {
                    Debug.WriteLine("\n Entra en 1 cambia estado a verdadero");
                    cierreTalanquera = true;
                    bonderaTalanquera1 = true;
                    pinSem2.Write(GpioPinValue.High);                    
                    pinTalanquera1.Write(GpioPinValue.High);
                    Task.Delay(200).Wait();
                    pinTalanquera1.Write(GpioPinValue.Low);

                }
            }
            else
            {                
                if (bonderaTalanquera1)
                {
                    Debug.WriteLine("\n Entra en 0 cambia estado a FALSO");
                    cierreTalanquera = false;
                    bonderaTalanquera1 = false;
                    pinSem2.Write(GpioPinValue.Low);
                    pinTalanquera1.Write(GpioPinValue.High);
                    Task.Delay(200).Wait();
                    pinTalanquera1.Write(GpioPinValue.Low);

                }
            }

            //Manejo del semaforo 3
            if (perifericos[2] == "1")
            {
                if (!bonderaTalanquera3)
                {
                    bonderaTalanquera3 = true;
                    cierreTalanquera3 = true;
                    pinSem3.Write(GpioPinValue.High);
                    pinTalanquera3.Write(GpioPinValue.High);
                    pinTalanquera2.Write(GpioPinValue.High);
                    Task.Delay(200).Wait();
                    pinTalanquera3.Write(GpioPinValue.Low);
                    pinTalanquera2.Write(GpioPinValue.Low);

                }
            }

            else
            {
                if (bonderaTalanquera3)
                {
                    pinSem3.Write(GpioPinValue.Low);
                    pinTalanquera3.Write(GpioPinValue.High);
                    pinTalanquera2.Write(GpioPinValue.High);
                    Task.Delay(200).Wait();
                    pinTalanquera3.Write(GpioPinValue.Low);
                    pinTalanquera2.Write(GpioPinValue.Low);
                    cierreTalanquera3 = false;
                    bonderaTalanquera3 = false;
                }
            }

            //Manejo del talarquera 1 
            //if (perifericos[3] == "1")
            //{

            //    if (!bonderaTalanquera1)
            //    {
            //        bonderaTalanquera1 = true;
            //        pinTalanquera1.Write(GpioPinValue.High);
            //        Task.Delay(200).Wait();
            //        pinTalanquera1.Write(GpioPinValue.Low);
            //    }

            //}

            //else
            //{
            //    if (bonderaTalanquera1)
            //    {
            //        bonderaTalanquera1 = false;
            //        pinTalanquera1.Write(GpioPinValue.High);
            //        Task.Delay(200).Wait();
            //        pinTalanquera1.Write(GpioPinValue.Low);
            //    }
            //}


            //Manejo del talarquera 2
            //if (perifericos[4] == "1")
            //{
            //    // PWM_L(pinTalanquera2);
            //    if (!bonderaTalanquera2)
            //    {
            //        bonderaTalanquera2 = true;
            //        pinTalanquera2.Write(GpioPinValue.High);
            //        Task.Delay(200).Wait();
            //        pinTalanquera2.Write(GpioPinValue.Low);
            //    }

            //}
            //else
            //{
            //    if (bonderaTalanquera2)
            //    {
            //        bonderaTalanquera2 = false;
            //        pinTalanquera2.Write(GpioPinValue.High);
            //        Task.Delay(200).Wait();
            //        pinTalanquera2.Write(GpioPinValue.Low);
            //    }
            //}

            //Manejo del talarquera 3
            //if (perifericos[5] == "1")
            //{
            //    // PWM_L(pinTalanquera2);
            //    if (!bonderaTalanquera3)
            //    {
            //        bonderaTalanquera3 = true;
            //        pinTalanquera3.Write(GpioPinValue.High);
            //        Task.Delay(200).Wait();
            //        pinTalanquera3.Write(GpioPinValue.Low);
            //    }

            //}
            //else
            //{
            //    if (bonderaTalanquera3)
            //    {
            //        bonderaTalanquera3 = false;
            //        pinTalanquera3.Write(GpioPinValue.High);
            //        Task.Delay(200).Wait();
            //        pinTalanquera3.Write(GpioPinValue.Low);
            //    }
            //}

            //Manejo del ALARMA SONORA 
            if (perifericos[8] == "1")
                pinAlarmaSonora.Write(GpioPinValue.High);
            else
                pinAlarmaSonora.Write(GpioPinValue.Low);




            //    socket.Send(estadoDispositivos);
        }

        private void socket_OnError(string message)
        {
            bonderaTalanquera1 = false;
            cierreTalanquera = false;
            bonderaTalanquera2 = false;
            bonderaTalanquera3 = false;
            // txtMensajes.Text += "\n " + message;

        }

        private void verificacionPerifericos()
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    pinSem1.Write(GpioPinValue.High);
            //    pinSem2.Write(GpioPinValue.High);
            //    pinSem3.Write(GpioPinValue.High);
            //    pinTalanquera1.Write(GpioPinValue.High);
            //    pinTalanquera2.Write(GpioPinValue.High);
            //    pinTalanquera3.Write(GpioPinValue.High);
            ////    Task.Delay(100).Wait();
            pinSem1.Write(GpioPinValue.Low);
            pinSem2.Write(GpioPinValue.Low);
            pinSem3.Write(GpioPinValue.Low);
            pinTalanquera1.Write(GpioPinValue.Low);
            pinTalanquera2.Write(GpioPinValue.Low);
            pinTalanquera3.Write(GpioPinValue.Low);
            //    Task.Delay(100).Wait();
            //}

        }

        public static void RestartHelper(ShutdownKind kind)
        {
            Debug.WriteLine("\n Se inicia proceso de reiniciar la aplicación!!!");
            ShutdownManager.BeginShutdown(kind, TimeSpan.FromSeconds(0));
        }

        public async void Serial()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector("UART0");                   
                var dis = await DeviceInformation.FindAllAsync(aqs);                    
                SerialDevice SerialPort = await SerialDevice.FromIdAsync(dis[0].Id);    

                /* Configure serial settings */
                SerialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                SerialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                SerialPort.BaudRate = 9600;                                             
                SerialPort.Parity = SerialParity.None;                                  
                SerialPort.StopBits = SerialStopBitCount.One;                           
                SerialPort.DataBits = 8;
                /* Write a string out over serial */
               // string txBuffer = "Hello Serial";
              //  DataWriter dataWriter = new DataWriter();
                /* Read data in from the serial port */
                const uint maxReadLength = 1024;
                DataReader dataReader = new DataReader(SerialPort.InputStream);
                while (true)
                {
                    //dataWriter.WriteString(txBuffer);
                    //uint bytesWritten = await SerialPort.OutputStream.WriteAsync(dataWriter.DetachBuffer());


                    uint bytesToRead = await dataReader.LoadAsync(maxReadLength);
                    Debug.WriteLine("Mensaje Recivido " + dataReader.ReadString(bytesToRead));
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ocurrio una excepción con el evento Serial " + ex.Message);
            }
        }


        /**
          METODOS DE CONTROL PARA SERVO MOTOR 
         **/

        public static void PWM_R(GpioPin pinNumber)
        {
            var stopwatch = Stopwatch.StartNew();

            workItemThread = Windows.System.Threading.ThreadPool.RunAsync(
                 (source) =>
                 {
                     // setup, ensure pins initialized
                     ManualResetEvent mre = new ManualResetEvent(false);
                     mre.WaitOne(1500);

                     ulong pulseTicks = ((ulong)(Stopwatch.Frequency) / 1000) * 2;
                     ulong delta;
                     var startTime = stopwatch.ElapsedMilliseconds;
                     while (stopwatch.ElapsedMilliseconds - startTime <= 300)
                     {
                         pinNumber.Write(GpioPinValue.High);
                         ulong starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks) break;
                         }
                         pinNumber.Write(GpioPinValue.Low);
                         starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks * 10) break;
                         }
                     }
                 }, WorkItemPriority.High);
        }

        public static void PWM_L(GpioPin pinNumber)
        {

            var stopwatch = Stopwatch.StartNew();

            workItemThread = Windows.System.Threading.ThreadPool.RunAsync(
                 (source) =>
                 {
                     // setup, ensure pins initialized
                     ManualResetEvent mre = new ManualResetEvent(false);
                     mre.WaitOne(1500);

                     ulong pulseTicks = ((ulong)(Stopwatch.Frequency) / 1000) * 2;
                     ulong delta;
                     var startTime = stopwatch.ElapsedMilliseconds;
                     while (stopwatch.ElapsedMilliseconds - startTime <= 300)
                     {
                         pinNumber.Write(GpioPinValue.High);
                         ulong starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = starttick - (ulong)(stopwatch.ElapsedTicks);
                             if (delta > pulseTicks) break;
                         }
                         pinNumber.Write(GpioPinValue.Low);
                         starttick = (ulong)(stopwatch.ElapsedTicks);
                         while (true)
                         {
                             delta = (ulong)(stopwatch.ElapsedTicks) - starttick;
                             if (delta > pulseTicks * 10) break;
                         }
                     }
                 }, WorkItemPriority.High);
        }

    }
}
