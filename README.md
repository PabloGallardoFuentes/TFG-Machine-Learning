# UTILIZACIÓN DEL ALGORITMO PARA OPTIMIZAR LA ASIGNACIÓN DE CONDUCTORES A UNA RUTA      
Para llevar a cabo la utilización de la funcionalidad completa se deben seguir distintos pasos, al menos la primera vez que se desee entrenar al modelo y guardarlo.

En los archivos subidos subidos a github faltan los siguientes por incapacidad de subirlos por su tamaño:

- <I>historic_repsol.csv</I>: historico inicial del que parte el primer filtrado
- <I>inputML.csv</I>: archivo generado después de realizar el filtrado inicial de <I>historic_repsol.csv</I>
- <I>100gruposCluster.csv</I>: condutor-ruta ya clasificados
- <I>forestRegressor.joblib</I>: archivo que contiene el modelo ya entrenado y que ahorra tiempo a la hora de posteriores ejecuciones

<details>
    <summary style="font-size:1.5em; font-weight:bold; margin-top:1em;">1. Procesamiento del set inicial</summary>
    Se realiza con el proyecto en .NET <I>FiltraCSVCompleto</I> que tarda alrededor de una hora en procesar todo el <I>historic_repsol.csv</I> y generar otro csv (normalmente denominado <I>inputML.csv</I>). Ambos archivos deberán estar en la carpta del proyecto <I>/bin/Debug/recursos/</I>.
</details>

<details>
    <summary style="font-size:1.5em; font-weight:bold; margin-top:1em;">2. Creación de los kilómetros en vacío (en caso de no existir) y filtrado de outsiders</summary>
    En caso de no tener los datos de kilómetros en vacío, se deben generar sintéticamente para el posterior modelado. Se necesita el archivo de <I>./recursos/inputML.csv</I> y se genera el archivo <I>./recursos/conKMsVacio.csv</I> y se utiliza el archivo <I>./generarKmsVacio.ipynb</I>. El tiempo de ejecución es breve.
</details>

<details>
    <summary style="font-size:1.5em; font-weight:bold; margin-top:1em;">3. Puntuar el histórico con algoritmo cluster</summary>
    Este archivo consigue generar una puntuación para todas las entradas de <I>./recursos/generaKMsVacio.csv</I> creando un archivo al cual se puede personalizar el nombre (normalmente <I>XgruposCluster.csv</I> siendo <I>X</I> sustituido por el numero de grupos que se han generado (parámetro configurable), por ejemplo, <I>./recursos/100gruposCluster.csv</I>). Además se puede cambiar la ponderación o importancia que tendrá cada característica dentro del modelo. También incluye ciertas visualizaciones de características a las que el ususario debe decidir si visualizarlas por su tiempo de ejecución
</details>

<details>
    <summary style="font-size:1.5em; font-weight:bold; margin-top:1em;">4. Entrenar modelo capaz de realizar predecicciones</summary>
    Este proceso es el entrenamiento final del algoritmo. Se realiza con el archivo <I>./supervisado.ipynb</I> y necesita el archivo previamente genereado, <I>./recursos/100gruposCluster.csv</I> o el nombre asignado. También guarda distintos archivos para su posterior uso sin la necesidad de realizar todo el proceso de entrenamiento:
    <ul>
    <li>Modelo entrenado: personalizable en los parámetros pero será un archivo .joblib. <I>forestRegressor.joblib</I> por ejemplo.</li>
    <li><I>conductores.csv</I>: contiene todos los identificadores de los conductores</li>
    <li>paradasCarga.csv<I></I>: contiene todos los identificadores de las paradas de carga que se han usado en el entrenamiento. Posteriormente se añadirán las coordenadas de cada parada</li>
    <li>paradasDescarga.csv<I></I>: contiene todos los identificadores de las paradas de descarga que se han usado en el entrenamiento. Posteriormente se añadirán las coordenadas de cada parada</li>
    </ul> 
    Además se puede visualizar el rendimiento del modelo en dos situciones:
    <ol>
    <li>Asignando una puntuación a todos los conductores existentes para una ruta concreta. Los kilómetros en vacío para cada conductor son generados aleatoriamente, además incluye valores con los que no ha sido entrenado el algoritmo para ver su rendimiento.</li>
    <li>Analizando como evoluciona la puntuacion de un condutor concreto para una ruta concreta en función de los kilómetros en vacío que debe realizar.</li>
    </ol>
He estado realizando más pruebas con los archivos denominados <I>deep.ipynb</I> y <I>deepSinCodigoInterno.ipynb</I> incluyendo las coordenadas de los puntos de parada y probando redes neuronales en estos casos. También se obtiene un buen rendimiento.
</details>

<details>
    <summary style="font-size:1.5em; font-weight:bold; margin-top:1em;">5. Guardar el modelo entrenado o cargarlo</summary>
</details>

