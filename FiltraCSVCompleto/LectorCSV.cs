using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltraCSVCompleto
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
        public string LatitudPuntoCarga { get; set; }
        public string LongitudPuntoCarga { get; set; }
        public string CodigoInternoPuntoDescarga { get; set; }
        public string LatitudPuntoDescarga { get; set; }
        public string LongitudPuntoDescarga { get; set; }
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
                    var values = linea.Split(';');

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

            FiltraDiccionarios(datosHistorico, paradasPorConductor);

            return new DictionariesContainer
            {
                RutasPorID = datosHistorico,
                RutasPorConductor = paradasPorConductor,
            };
        }

        private static void FiltraDiccionarios(Dictionary<string, List<DatoHistorico>> datosHistorico, Dictionary<string, List<DatoHistorico>> paradasPorConductor)
        {
            List<string> listaRutasABorrar = new List<string>();
            List<string> listaConductoresABorrar = new List<string>();
            int i = 0;
            foreach (string conductor in paradasPorConductor.Keys)
            {
                int nRutas = NumeroDeRutasDeUnConductor(paradasPorConductor, conductor);
                if (nRutas > 1000 || nRutas < 100 || conductor.Equals(""))
                {
                    // list de rutas de ese conductor
                    foreach (DatoHistorico parada in paradasPorConductor[conductor])
                    {
                        if (!listaRutasABorrar.Contains(parada.IDRutaARealizar))
                        {
                            listaRutasABorrar.Add(parada.IDRutaARealizar);
                        }
                    }

                    //anhade en lista de borrar al conductor
                    listaConductoresABorrar.Add(conductor);
                }
                else
                {
                    Console.WriteLine("conductor fuera del rango o vacio" + i);
                }

                if (i++ % 100 == 0) Console.WriteLine("Filtrando " + i + " de " + paradasPorConductor.Count);
            }

            foreach (string idRuta in listaRutasABorrar)
                datosHistorico.Remove(idRuta);

            foreach (string idConductor in listaConductoresABorrar)
                paradasPorConductor.Remove(idConductor);

            /*
            return new DictionariesContainer
            {
                RutasPorID = datosHistorico,
                RutasPorConductor = paradasPorConductor,
            };*/
        }

        private static void AddToDictionary(Dictionary<string, List<DatoHistorico>> dict, string key, DatoHistorico value)
        {
            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<DatoHistorico>();
            }
            dict[key].Add(value);
        }



        public static List<DatoML> TransformaHistoricoEnInputML(Dictionary<string, List<DatoHistorico>> datosHistorico,
           Dictionary<string, List<DatoHistorico>> paradasPorConductor)
        {
            List<DatoML> inputsML = new List<DatoML>();
            int paso = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var key in datosHistorico.Keys)
            {
                DatoML inputML = new DatoML();
                List<DatoHistorico> paradas = datosHistorico[key];
                int fechaInicioIndex = 0;
                int fechaFinIndex = 1;

                if (paradas.Count >= 2)
                {
                    if (DateTime.Parse(paradas[fechaFinIndex].FechaPlanificada) < DateTime.Parse(paradas[fechaInicioIndex].FechaPlanificada))
                    {
                        //Console.WriteLine("Cambio de fecha: ini:"+fechaInicio.ToString().PadRight(10)+ "fin:"+fechaFin.ToString());
                        var aux = fechaInicioIndex;
                        fechaInicioIndex = fechaFinIndex;
                        fechaFinIndex = aux;
                        //Console.WriteLine("Nueva fecha: ini:" + paradas[fechaInicioIndex].FechaParada.PadRight(10) + "fin:" + paradas[fechaFinIndex].FechaParada);
                    }

                    //Console.WriteLine("numero de paradas" + paradas.Count + " de la ruta " + key);
                    // en caso de tener mas de dos paradas, se comprueba primera y ultima 
                    for (int i = 2; i < paradas.Count; i++)
                    {
                        //primerafecha punto de carga, ultima fecha punto descarga
                        if (DateTime.Parse(paradas[i].FechaPlanificada) < DateTime.Parse(paradas[fechaInicioIndex].FechaPlanificada))
                        {
                            fechaInicioIndex = i;
                        }
                        else if (DateTime.Parse(paradas[fechaFinIndex].FechaPlanificada) < DateTime.Parse(paradas[i].FechaPlanificada))
                        {
                            fechaFinIndex = i;
                        }
                    }

                    inputML.IDConductor = paradas[fechaInicioIndex].IDConductor;
                    //parada carga
                    inputML.CodigoInternoPuntoCarga = paradas[fechaInicioIndex].CodigoInterno;
                    inputML.LatitudPuntoCarga = paradas[fechaInicioIndex].LatitudParada;
                    inputML.LongitudPuntoCarga = paradas[fechaInicioIndex].LongitudParada;
                    //parada descarga
                    inputML.CodigoInternoPuntoDescarga = paradas[fechaFinIndex].CodigoInterno;
                    inputML.LatitudPuntoDescarga = paradas[fechaFinIndex].LatitudParada;
                    inputML.LongitudPuntoDescarga = paradas[fechaFinIndex].LongitudParada;
                    //caracteristicas
                    int nRutas = NumeroDeRutasDeUnConductor(paradasPorConductor, paradas[fechaInicioIndex].IDConductor);
                    inputML.NumeroDeRutasDelConductor = nRutas;
                    inputML.PorcentajePuntosDeRuta = PorcentajeVecesEnPuntoDeParadasDeUnConductor(
                        paradas[fechaInicioIndex], paradas[fechaFinIndex], paradasPorConductor[paradas[fechaInicioIndex].IDConductor]);
                    inputML.Label = 0;
                    inputML.FechaEsperadaFinDeRuta = paradas[fechaFinIndex].FechaPlanificada;
                    inputML.FechaRealFinDeRuta = paradas[fechaFinIndex].FechaRealLlegada;


                    if (inputML.FechaEsperadaFinDeRuta != "" && inputML.FechaRealFinDeRuta != "")
                        inputsML.Add(inputML);
                }
                /*else
                {
                    Console.WriteLine("Ruta con menos de dos paradas");
                }*/
                if (paso % 100 == 0)
                {
                    Console.WriteLine("generando inputML " + paso + " de " + datosHistorico.Keys.Count);
                }
                paso++;
            }
            stopwatch.Stop();

            Console.WriteLine("input ml tam: " + inputsML.Count.ToString().PadRight(10)
                + "Tiempo: " + stopwatch.Elapsed.TotalMinutes + ":" + stopwatch.Elapsed.TotalSeconds);

            return inputsML;
        }


        public static void SacaCSVParaML(List<DatoML> inputsML, string archivo)
        {
            string stringCSV = "IDconductor,CodigoInternoPuntoCarga,LatitudPuntoCarga,LongitudPuntoCarga,CodigoInternoPuntoDescarga,LatitudPuntoDescarga,LongitudPuntoCarga,NumParadas,PorcentajePunto,Retraso\n";
            int i = 0;
            foreach (DatoML input in inputsML)
            {
                if (input.IDConductor != "")
                {
                    double retraso = DateTime.Parse(input.FechaRealFinDeRuta).Subtract(DateTime.Parse(input.FechaEsperadaFinDeRuta)).TotalHours;
                    stringCSV += string.Join(",", new string[] { input.IDConductor,
                        input.CodigoInternoPuntoCarga, input.LatitudPuntoCarga.Replace(",", "."), input.LongitudPuntoCarga.Replace(",", "."),
                        input.CodigoInternoPuntoDescarga, input.LatitudPuntoDescarga.Replace(",", "."), input.LongitudPuntoDescarga.Replace(",", "."),
                        input.NumeroDeRutasDelConductor.ToString(), input.PorcentajePuntosDeRuta.ToString().Replace(",", "."),
                        retraso.ToString().Replace(",", ".") + '\n'});
                }

                if (i % 100 == 0)
                    Console.WriteLine("Escribiendo: " + i + " de " + inputsML.Count);
                i++;
            }

            //Console.WriteLine(stringCSV);
            // Crear o sobrescribir el archivo CSV y escribir los datos
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recursos", archivo);
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(stringCSV);
            }
        }


        private static int NumeroDeRutasDeUnConductor(Dictionary<string, List<DatoHistorico>> rutasPorConductor, string IDConductor)
        {
            List<DatoHistorico> paradasConductor = rutasPorConductor[IDConductor];
            List<string> IDsRutasContadas = new List<string>();

            foreach (DatoHistorico parada in paradasConductor)
            {
                if (!IDsRutasContadas.Contains(parada.IDRutaARealizar))
                {
                    IDsRutasContadas.Add(parada.IDRutaARealizar);
                }
            }
            return IDsRutasContadas.Count;
        }

        private static double PorcentajeVecesEnPuntoDeParadasDeUnConductor(DatoHistorico paradaInicialRuta, DatoHistorico paradaFinalRuta,
            List<DatoHistorico> paradasDeConductor)
        {
            double totalParadas = paradasDeConductor.Count;
            double totalVecesEnAlgunPuntoDeLaRuta = 0;

            foreach (DatoHistorico parada in paradasDeConductor)
            {
                if (parada.CodigoInterno.Equals(paradaInicialRuta.CodigoInterno)
                    || parada.CodigoInterno.Equals(paradaFinalRuta.CodigoInterno))
                {
                    totalVecesEnAlgunPuntoDeLaRuta++;
                }
            }


            return (double)(totalVecesEnAlgunPuntoDeLaRuta / totalParadas);
        }
    }

}
