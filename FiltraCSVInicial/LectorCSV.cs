using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltraCSVInicial
{
    public class DictionariesContainer
    {
        public Dictionary<string, List<DatoHistorico>> RutasPorID { get; set; }
        public Dictionary<string, List<DatoHistorico>> RutasPorConductor { get; set; }
    }

    public class DatoHistorico
    {
        public string IDRutaARealizar { get; set; }
        public string IDConductor { get; set; }
        public string Matricula1 { get; set; }
        public string Matricula2 { get; set; }
        public string Matricula3 { get; set; }
        public string CodigoInterno { get; set; }
        public string TipoParada { get; set; }
        public string FechaPlanificada { get; set; }
        public string FechaRealLlegada { get; set; }
        public string LatitudParada { get; set; }
        public string LongitudParada { get; set; }
    }

    public class DatoML
    {
        //Ruta
        public string CodigoInternoPuntoCarga { get; set; }
        public string CodigoInternoPuntoDescarga { get; set; }
        public string FechaEsperadaFinDeRuta { get; set; }
        public string FechaRealFinDeRuta { get; set; }


        //Conductor
        public string IDConductor { get; set; }
        public int NumeroDeRutasDelConductor { get; set; }
        public double PorcentajePuntosDeRuta { get; set; }

        //Etiqueta / puntuacion
        public int Label { get; set; }
    }
    public static class LectorCSV
    {
        public static DictionariesContainer leeHistorico()
        {
            Dictionary<string, List<DatoHistorico>> datosHistorico = new Dictionary<string, List<DatoHistorico>>();
            Dictionary<string, List<DatoHistorico>> paradasPorConductor = new Dictionary<string, List<DatoHistorico>>();

            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recursos", "historic_repsol.csv");
            //Console.WriteLine(ruta);

            using (var lector = new StreamReader(ruta))
            {
                //Cabecera
                lector.ReadLine();

                while (!lector.EndOfStream)
                {
                    var linea = lector.ReadLine().Replace("    ", "");
                    //Console.WriteLine(linea);
                    var values = linea.Split(',');

                    var nuevoDato = new DatoHistorico
                    {
                        IDRutaARealizar = values[0],
                        IDConductor = values[1],
                        Matricula1 = values[2],
                        Matricula2 = values[3],
                        Matricula3 = values[4],
                        CodigoInterno = values[5],
                        TipoParada = values[6],
                        FechaPlanificada = values[7],
                        FechaRealLlegada = values[8],
                        LatitudParada = values[11],
                        LongitudParada = values[12]
                    };

                    AddToDictionary(datosHistorico, nuevoDato.IDRutaARealizar, nuevoDato);
                    AddToDictionary(paradasPorConductor, nuevoDato.IDConductor, nuevoDato);
                }
            }

            return new DictionariesContainer
            {
                RutasPorID = datosHistorico,
                RutasPorConductor = paradasPorConductor,
            };
        }

        private static void AddToDictionary(Dictionary<string, List<DatoHistorico>> dict, string key, DatoHistorico value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<DatoHistorico>();
            }
            dict[key].Add(value);
        }
    }
}
