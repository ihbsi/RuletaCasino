using System;
using System.IO;

namespace RuletaCasino.WebApi
{
    public class LogTrace
    {
        private String sArchLog = null;
        private Boolean bLog = false;

        public LogTrace()
        {
        }

        /// <summary>
        /// Constructor de clase onde indica si la ruta del archivo de traza y si esta activo o no
        /// </summary>
        /// <param name="sFilePath">Nombre del archivo txt de traza</param>
        /// <param name="bOnOff">Verdadero para activar el log de traza y falso para inactivar el log de traza</param>
        public LogTrace(String sFilePath, Boolean bOnOff)
        {
            sArchLog = sFilePath;
            bLog = bOnOff;
        }

        /// <summary>
        /// Metodo que escribe la traza en el archivo
        /// </summary>
        /// <param name="sMetodo">Metodo de donde se genera la traza</param>
        /// <param name="sFuncion">Funcion donde se genera la traza</param>
        /// <param name="sMensaje">Mensaje </param>
        public void WriteLog(String sMetodo,
                             String sFuncion,
                             String sMensaje)
        {
            try
            {
                if (!bLog) return;
                StreamWriter sw = File.AppendText(@sArchLog);
                DateTime f = DateTime.Now;
                String sFecha = f.ToString("dd/MM/yyyy HH:mm:ss");
                sMensaje = sFecha + ";" + sMetodo + ";" + sFuncion + ";" + sMensaje + ";";
                sw.WriteLine(sMensaje);
                sw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public String ArchLog
        {
            get { return sArchLog; }
            set { sArchLog = value; }
        }

        public Boolean ActivarLog
        {
            get { return bLog; }
            set { bLog = value; }
        }
    }
}