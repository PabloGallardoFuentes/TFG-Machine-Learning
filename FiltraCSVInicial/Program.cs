using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace FiltraCSVInicial
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Ejecutar el filtado de repsol? (+40 minutos) (s/n): ");
            string respuesta = Console.ReadLine();
            if (respuesta.Equals("s"))
            {
                DictionariesContainer diccionarios = LectorCSV.leeHistorico();
                diccionarios = FiltroCSV.FiltraDiccionarios(diccionarios.RutasPorID, diccionarios.RutasPorConductor);
                List<DatoML> lista = FiltroCSV.TransformaHistoricoEnInputML(diccionarios.RutasPorID, diccionarios.RutasPorConductor);

                FiltroCSV.SacaCSVParaML(lista, "inputML.csv");
            } else
            {
                Console.WriteLine("Busca el archivo filtrado en ./bin/Debug/recursos/");
            }
        }
    }
}
