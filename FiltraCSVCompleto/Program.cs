using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltraCSVCompleto
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Ejecutar el filtrado de repsol? (+1 hora) (s/n): ");
            string respuesta = Console.ReadLine();
            if (respuesta.Equals("s"))
            {
                DictionariesContainer diccionarios = LectorCSV.leeHistorico();
                List<DatoML> inputsML = LectorCSV.TransformaHistoricoEnInputML(diccionarios.RutasPorID, diccionarios.RutasPorConductor);

                LectorCSV.SacaCSVParaML(inputsML, "inputML.csv");
            } else
            {
                Console.WriteLine("Busca el archivo filtrado en ./bin/Debug/recursos/");
            }
        }
    }
}
