using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltraCSVInicial
{
    public static class FiltroCSV
    {
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
                    inputML.CodigoInternoPuntoCarga = paradas[fechaInicioIndex].CodigoInterno;
                    inputML.CodigoInternoPuntoDescarga = paradas[fechaFinIndex].CodigoInterno;
                    int nRutas = NumeroDeRutasDeUnConductor(paradasPorConductor, paradas[fechaInicioIndex].IDConductor);
                    inputML.NumeroDeRutasDelConductor = nRutas;
                    inputML.PorcentajePuntosDeRuta = PorcentajeVecesEnPuntoDeParadasDeUnConductor(
                        paradas[fechaInicioIndex], paradas[fechaFinIndex], paradasPorConductor[paradas[fechaInicioIndex].IDConductor]);
                    inputML.Label = 0;
                    inputML.FechaEsperadaFinDeRuta = paradas[fechaFinIndex].FechaPlanificada;
                    inputML.FechaRealFinDeRuta = paradas[fechaFinIndex].FechaRealLlegada;

                    /*if (paradas[fechaInicioIndex].IDConductor.Equals("DAT679181"))
                        Console.WriteLine("conductor: " + paradas[fechaInicioIndex].IDConductor.PadRight(15) + "rutas: " + 
                            nRutas.ToString().PadRight(8) + "porc: "+ inputML.PorcentajePuntosDeRuta.ToString().PadRight(8) +
                            "ini:"+paradas[fechaInicioIndex].CodigoInterno.PadRight(10) + 
                            "fin:"+paradas[fechaFinIndex].CodigoInterno);*/

                    if (inputML.FechaEsperadaFinDeRuta != "" && inputML.FechaRealFinDeRuta != "")
                        inputsML.Add(inputML);
                }
                /*else
                {
                    Console.WriteLine("Ruta con menos de dos paradas");
                }*/
                if (paso % 1000 == 0)
                {
                    Console.WriteLine("generando inputML " + paso);
                }
                paso++;
            }
            stopwatch.Stop();

            Console.WriteLine("input ml tam: " + inputsML.Count.ToString().PadRight(10)
                + "Tiempo: " + stopwatch.Elapsed.TotalMinutes + ":" + stopwatch.Elapsed.TotalSeconds);

            return inputsML;
        }

        public static DictionariesContainer FiltraDiccionarios(Dictionary<string, List<DatoHistorico>> datosHistorico, Dictionary<string, List<DatoHistorico>> paradasPorConductor)
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
                    //Console.WriteLine("conductor fuera del rango o vacio" + i);
                }

                if (i++ % 100 == 0) Console.WriteLine("Filtrando " + i + " de " + paradasPorConductor.Count);
            }

            foreach (string idRuta in listaRutasABorrar)
                datosHistorico.Remove(idRuta);

            foreach (string idConductor in listaConductoresABorrar)
                paradasPorConductor.Remove(idConductor);

            return new DictionariesContainer
            {
                RutasPorID = datosHistorico,
                RutasPorConductor = paradasPorConductor,
            };
        }


        public static void SacaCSVParaML(List<DatoML> inputsML, string archivo)
        {
            string stringCSV = "IDConductor,CodigoInternoPuntoCarga,CodigoInternoPuntoDescarga,Label,NumeroParadas,PorcenajePuntoConocido\n";
            int i = 0;
            foreach (DatoML input in inputsML)
            {
                if (input.IDConductor != "")
                {
                    stringCSV += string.Join(",", new string[] { input.IDConductor, input.CodigoInternoPuntoCarga,
                        input.CodigoInternoPuntoDescarga, 0.ToString(), input.NumeroDeRutasDelConductor.ToString(),
                    input.PorcentajePuntosDeRuta.ToString()}) + "\n";
                }

                if(i %100 == 0)
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
