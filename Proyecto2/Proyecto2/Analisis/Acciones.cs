using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

using Proyecto2.TablaSimbolos;

namespace Proyecto2.Analisis
{
    class Acciones
    {

        static int Nivel = 0;
        static int contadorambitos = 0;
        static String Print = "";

        //variables 
        static String IdVariable = "";
        static String TipoVariable = "";
        static String Valovariable = "";
        static String AmbitoVariable = "Global";
        static List<String> listaids = new List<string>();

        //Metodos y Funciones 
        static String IdMetodo = "";
        static String TipoMetodo = "";
        static ParseTreeNode Return = null;
        static ParseTreeNode Cuerpo = null;
        static ParseTreeNode NodoParametros = null;


        static List<String> listaparametros = new List<string>();
        public static List<String> Impresiones = new List<String>();
        static List<MetodosYFuncion> Tablafunciones = new List<MetodosYFuncion>();      //en la primera pasada almacenaremos las funciones
        static List<Simbolos> TablaSimbolos = new List<Simbolos>();                     //lista de variables que vayamos reconociendo
        static List<Ambitos> Amb = new List<Ambitos>();                                 //lista que funcionara como pila de ambitos

        public static void RealizarAccionesAcciones(ParseTreeNode Raiz)
        {

            #region LIMPIEZA DE VARIABLES
            Nivel = 0;
            contadorambitos = 0;
            Print = "";
            IdVariable = "";
            TipoVariable = "";
            Valovariable = "";
            AmbitoVariable = "Global";

            listaparametros.Clear();
            Impresiones.Clear();
            TablaSimbolos.Clear();
            listaids.Clear();
            Amb.Clear();
            Tablafunciones.Clear();

            #endregion

            PrimeraPasada(Raiz);            //reconocimiento de variables globales y metodos y funciones

            for (int i = 0; i < Tablafunciones.Count; i++)  //Buscamos el metodo main para empezar la ejecucion de codigo.
            {
                MetodosYFuncion metodo = (MetodosYFuncion)Tablafunciones[i];
                if (metodo.getNombre().Equals("Main") || metodo.getNombre().Equals("main"))
                {
                    AmbitoVariable = "Main";
                    Nivel++;
                    //creamos un nuevo ambito
                    Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));
                    EjecucionCodigo(metodo.getParametros(), AmbitoVariable, Nivel);
                    EjecucionCodigo(metodo.getCuerpo(), AmbitoVariable, Nivel);

                    // DesapilarAmbito(AmbitoVariable,Nivel);

                    break;
                }
            }

            #region IMPRESIONES DE PRUEBA
            for (int i = 0; i < Amb.Count; i++)
            {
                Ambitos simb = (Ambitos)Amb[i];

                List<Simbolos> si = simb.getSimbolos();

                Console.WriteLine(simb.getAmbito() + " Nivel: " + simb.getNivel() + " Variables:");
                for (int j = 0; j < si.Count; j++)
                {
                    Simbolos yeah = (Simbolos)si[j];
                    Console.WriteLine(yeah.GetId() + ":" + yeah.GetTipo() + "=" + yeah.GetValor() + "; -> Nivel: " + yeah.Getnivel() + " Ambito: " + yeah.GetAmbito());
                }
            }

            /* for (int i = 0; i < Tablafunciones.Count; i++)
             {
                 MetodosYFuncion simb = (MetodosYFuncion)Tablafunciones[i];

                 Console.WriteLine(simb.getNombre() + " Tipo: " + simb.getTipo() + ".");

             }*/
            #endregion
        }

        public static void DesapilarAmbito(String Ambito, int niv)
        {
            for (int i = 0; i < Amb.Count; i++)
            {
                Ambitos simb = (Ambitos)Amb[i];
                if (simb.getAmbito().Equals(Ambito) && simb.getNivel() == niv)
                {
                    Amb.RemoveAt(i);
                }
            }
        }

        public static void PrimeraPasada(ParseTreeNode Nodo)    //Primera fase
        {

            switch (Nodo.Term.Name.ToString())
            {
                case "S":     // S.Rule = IMPORTS + INICIO;                    
                    String AmbitoAnterior = AmbitoVariable;
                    AmbitoVariable = "Global";
                    Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        PrimeraPasada(Nodo.ChildNodes[i]);
                    }

                    //DesapilarAmbito(AmbitoVariable,Nivel);      //desapilamos el ambito de la pila de ambitos
                    AmbitoVariable = AmbitoAnterior;            //regreso al ambito donde se encontraba
                    AmbitoAnterior = "";

                    break;

                case "INICIO":  //INICIO.Rule = INICIO + ACCIONES
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        PrimeraPasada(Nodo.ChildNodes[i]);
                    }
                    break;
                case "ACCIONES":
                    /* ACCIONES.Rule =  DECLARACIONVARIABLES
                                        | DECLARACIONMETODOS
                                        | DECLARACIONARREGLOS
                                        | ASIGNACIONARREGLO
                                        | ASIGNACIONVAR;*/

                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        if (Nodo.ChildNodes[i].Term.Name.ToString().Equals("DECLARACIONVARIABLES"))
                        {
                            for (int z = 0; z < Amb.Count; z++)
                            {
                                Ambitos amb = (Ambitos)Amb[z];
                                if (amb.getAmbito().Equals("Global") && amb.getNivel() == Nivel)
                                {
                                    DeclaracionVariables(Nodo.ChildNodes[i], amb.getSimbolos());
                                }
                            }
                            // Aumentamos el nivel para los demas ambitos.
                        }
                        else if (Nodo.ChildNodes[i].Term.Name.ToString().Equals("DECLARACIONMETODOS"))
                        {
                            ReconocimientoMetodosYFunciones(Nodo.ChildNodes[i]);
                        }
                        else if (Nodo.ChildNodes[i].Term.Name.ToString().Equals("ASIGNACIONVAR"))   //   asignacion de variables de forma global
                        {
                            String id = Tipos_Y_Id(Nodo.ChildNodes[i].ChildNodes[0]);
                            String valor = Condiciones(Nodo.ChildNodes[i].ChildNodes[2]);

                            for (int x = 0; x < Amb.Count; x++)
                            {
                                Ambitos simb = (Ambitos)Amb[x];
                                if (simb.getNivel() == Nivel && simb.getAmbito().Equals("Global"))
                                {
                                    List<Simbolos> si = simb.getSimbolos();
                                    for (int l = 0; l < si.Count; l++)
                                    {
                                        Simbolos yeah = (Simbolos)si[l];
                                        if (yeah.GetId().Equals(id))
                                        {
                                            yeah.SetValor(valor);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            PrimeraPasada(Nodo.ChildNodes[i]);
                        }
                    }
                    break;
            }
        }

        public static void EjecucionCodigo(ParseTreeNode Nodo, String Ambito, int Niv)
        {
            switch (Nodo.Term.Name.ToString())
            {
                case "LISTAPARAMETROS":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        EjecucionCodigo(Nodo.ChildNodes[i], Ambito, Nivel);
                    }
                    break;
                case "PARAMETRO":
                    if (Nodo.ChildNodes.Count > 0)
                    {
                        String Idparametro = Tipos_Y_Id(Nodo.ChildNodes[0]);
                        String Tipoparametro = Tipos_Y_Id(Nodo.ChildNodes[2].ChildNodes[0]);
                        String ValorParametro = "";
                        switch (Tipoparametro)
                        {
                            case "Int":
                                ValorParametro = "0";
                                break;
                            case "Boolean":
                                ValorParametro = "false";
                                break;
                            case "Double":
                                ValorParametro = "0.0";
                                break;
                            case "String":
                                ValorParametro = "\"\"";
                                break;
                            case "Char":
                                ValorParametro = "'\u0000'";
                                break;
                        }
                        //realizamos la insercion de la variable.
                        for (int i = 0; i < Amb.Count; i++)
                        {
                            Ambitos amb = (Ambitos)Amb[i];
                            if (amb.getAmbito().Equals(Ambito) && amb.getNivel() == Nivel)
                            {
                                List<Simbolos> sim = amb.getSimbolos();
                                sim.Add(new Simbolos(Idparametro, Ambito, Nivel, ValorParametro, Tipoparametro));
                                amb.setSimbolos(sim);
                            }
                        }
                    }
                    break;

                case "LISTASENTENCIAS":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        EjecucionCodigo(Nodo.ChildNodes[i], Ambito, Nivel);
                    }
                    break;
                case "SENTENCIAS":
                    if (Nodo.ChildNodes.Count > 0)
                    {
                        EjecucionCodigo(Nodo.ChildNodes[0], Ambito, Nivel);
                    }
                    break;
                case "ACCIONES":
                    EjecucionCodigo(Nodo.ChildNodes[0], Ambito, Nivel);
                    break;
                case "DECLARACIONVARIABLES":
                    for (int i = 0; i < Amb.Count; i++)
                    {
                        Ambitos amb = (Ambitos)Amb[i];
                        if (amb.getAmbito().Equals(Ambito) && amb.getNivel() == Nivel)
                        {
                            DeclaracionVariables(Nodo, amb.getSimbolos());
                        }
                    }
                    break;
                case "ASIGNACIONVAR":
                    String id = Tipos_Y_Id(Nodo.ChildNodes[0]);
                    String valor = Condiciones(Nodo.ChildNodes[2]);

                    for (int x = 0; x < Amb.Count; x++)
                    {
                        Ambitos simb = (Ambitos)Amb[x];
                        if (simb.getNivel() == Nivel)
                        {
                            List<Simbolos> si = simb.getSimbolos();
                            for (int l = 0; l < si.Count; l++)
                            {
                                Simbolos yeah = (Simbolos)si[l];
                                if (yeah.GetId().Equals(id))
                                {
                                    yeah.SetValor(valor);
                                }
                            }
                        }
                    }
                    break;

                #region LLAMADA
                case "LLAMADA":
                    String AmbitoAnterior100 = AmbitoVariable;

                    String Idmetodollamar = Tipos_Y_Id(Nodo.ChildNodes[0]);
                    EjecucionCodigo(Nodo.ChildNodes[2], Ambito, Nivel);     //mandamos a actualizar el dato de sus parametros. y luego las acciones

                    for (int i = 0; i < Tablafunciones.Count; i++)  //Buscamos el metodo main para empezar la ejecucion de codigo.
                    {
                        MetodosYFuncion metodo = (MetodosYFuncion)Tablafunciones[i];
                        if (metodo.getNombre().Equals(Idmetodollamar))
                        {
                            AmbitoVariable = Idmetodollamar + contadorambitos;
                            contadorambitos++;
                            Niv++;
                            Nivel = Niv;

                            //creamos un nuevo ambito
                            Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));
                            EjecucionCodigo(metodo.getParametros(), AmbitoVariable, Nivel);

                            for (int x = 0; x < Amb.Count; x++)
                            {
                                Ambitos simb = (Ambitos)Amb[x];
                                if (simb.getNivel() == Nivel && simb.getAmbito().Equals(AmbitoVariable))
                                {
                                    List<Simbolos> si = simb.getSimbolos();
                                    int cantparametros = listaparametros.Count;
                                    for (int l = 0; l < cantparametros; l++)
                                    {
                                        si[l].SetValor(listaparametros[l]);
                                    }
                                    simb.setSimbolos(si);
                                    listaparametros.Clear();
                                }
                            }

                            EjecucionCodigo(metodo.getCuerpo(), AmbitoVariable, Nivel);

                            DesapilarAmbito(AmbitoVariable, Nivel);
                            AmbitoVariable = AmbitoAnterior100;
                            Niv--;
                            Nivel = Niv;
                            contadorambitos--;
                            break;
                        }
                    }

                    break;
                case "PAR":
                case "LISTAPARAMETROSLLAMADA":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        EjecucionCodigo(Nodo.ChildNodes[i], Ambito, Nivel);
                    }
                    break;
                case "CONDICIONES":

                    String para = Condiciones(Nodo.ChildNodes[0]);
                    listaparametros.Add(para);

                    break;

                #endregion

                #region PRINT
                case "PRINT":
                    //PRINT.Rule = tkPRINT + tkPARA + LISTAEXPRESIONES + tkPARC + tkPUNTOYCOMA
                    Print = "";
                    EjecucionCodigo(Nodo.ChildNodes[2], Ambito, Nivel);
                    Impresiones.Add(Print);

                    break;
                case "LISTAEXPRESIONES":
                    /* LISTAEXPRESIONES.Rule = LISTAEXPRESIONES + tkCOMA + OPERACION
                                    | OPERACION;*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        EjecucionCodigo(Nodo.ChildNodes[0], Ambito, Nivel);
                        Print += Operaciones(Nodo.ChildNodes[2]);
                    }
                    else
                    {
                        Print += Operaciones(Nodo.ChildNodes[0]);
                    }
                    break;

                case "OPERACION":
                    Print += Operaciones(Nodo);
                    break;
                #endregion

                #region IF-ELSE
                case "IF":
                    String AmbitoAnterior = AmbitoVariable;
                    AmbitoVariable = "IF" + contadorambitos;
                    contadorambitos++;
                    Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                    String condi = Condiciones(Nodo.ChildNodes[2]);         //si condiciones es true se mete al if  de lo contrario se mete al else

                    if (Convert.ToBoolean(condi))       //condicion true
                    {
                        EjecucionCodigo(Nodo.ChildNodes[5], "IF", Nivel);
                    }
                    else                               //condicion false
                    {
                        if (Nodo.ChildNodes.Count == 8)          //esto debido a que el bloque else puede o no venir 
                        {//si existe bloque else que entre

                            EjecucionCodigo(Nodo.ChildNodes[7], "ELSE", Nivel);
                        }
                    }

                    DesapilarAmbito(AmbitoVariable, Nivel);      //desapilamos el ambito de la pila de ambitos
                    contadorambitos--;
                    AmbitoVariable = AmbitoAnterior;            //regreso al ambito donde se encontraba
                    AmbitoAnterior = "";

                    break;
                case "BLOQUEELSE":
                    if (Nodo.ChildNodes.Count > 0)
                    {
                        String AmbitoAnterior1 = AmbitoVariable;
                        AmbitoVariable = "ELSE" + contadorambitos;
                        contadorambitos++;
                        Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                        EjecucionCodigo(Nodo.ChildNodes[2], "ELSE", Nivel);

                        DesapilarAmbito(AmbitoVariable, Nivel);      //desapilamos el ambito de la pila de ambitos
                        contadorambitos--;
                        AmbitoVariable = AmbitoAnterior1;            //regreso al ambito donde se encontraba
                        AmbitoAnterior = "";
                    }
                    break;
                #endregion

                #region WHILE
                case "WHILE":
                    // WHILE.Rule = tkWHILE + tkPARA + CONDICIONES + tkPARC + tkLLAVA + LISTASENTENCIAS + tkLLAVC;
                    String AmbitoAnterior2 = AmbitoVariable;
                    AmbitoVariable = "WHILE" + contadorambitos;
                    contadorambitos++;
                    Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                    String condi1 = "";
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        if (i == 2)
                        {
                            condi1 = Condiciones(Nodo.ChildNodes[2]);         //si condiciones es true se mete al if  de lo contrario se mete al else

                            if (!Convert.ToBoolean(condi1))       //condicion true
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (i == 5 && Convert.ToBoolean(condi1))
                            {
                                while (Convert.ToBoolean(condi1))
                                {
                                    EjecucionCodigo(Nodo.ChildNodes[i], AmbitoVariable, Nivel);
                                    EjecucionCodigo(Nodo, AmbitoVariable, Nivel);
                                    break;
                                }
                            }
                        }
                    }
                    DesapilarAmbito(AmbitoVariable, Nivel);      //desapilamos el ambito de la pila de ambitos
                    contadorambitos--;
                    AmbitoVariable = AmbitoAnterior2;            //regreso al ambito donde se encontraba
                    AmbitoAnterior = "";
                    break;
                #endregion

                #region  DO-WHILE
                case "DO":
                    //   DO.Rule = tkDO + tkLLAVA + LISTASENTENCIAS + tkLLAVC + tkWHILE + tkPARA + CONDICIONES + tkPARC + tkPUNTOYCOMA;
                    String AmbitoAnterior3 = AmbitoVariable;
                    AmbitoVariable = "DOWHILE" + contadorambitos;
                    contadorambitos++;
                    Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                    String condi2 = "";
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        if (i == 6)
                        {
                            condi2 = Condiciones(Nodo.ChildNodes[6]);         //si condiciones es true se mete al if  de lo contrario se mete al else

                            if (Convert.ToBoolean(condi2))       //condicion true
                            {
                                EjecucionCodigo(Nodo, AmbitoVariable, Nivel);
                            }
                        }
                        else
                        {
                            EjecucionCodigo(Nodo.ChildNodes[i], AmbitoVariable, Nivel);
                        }
                    }

                    DesapilarAmbito(AmbitoVariable, Nivel);      //desapilamos el ambito de la pila de ambitos
                    contadorambitos--;
                    AmbitoVariable = AmbitoAnterior3;            //regreso al ambito donde se encontraba
                    AmbitoAnterior = "";
                    break;
                    #endregion

            }
        }

        public static void ReconocimientoMetodosYFunciones(ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name.ToString())
            {
                case "DECLARACIONMETODOS":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        ReconocimientoMetodosYFunciones(Nodo.ChildNodes[i]);
                    }

                    Tablafunciones.Add(new MetodosYFuncion(IdMetodo, TipoMetodo, Cuerpo, NodoParametros, Return));
                    IdMetodo = "";
                    TipoMetodo = "";
                    Cuerpo = null;
                    NodoParametros = null;
                    Return = null;

                    break;
                case "LISTASENTENCIAS":
                    Cuerpo = Nodo;
                    break;
                case "LISTAPARAMETROS":
                    NodoParametros = Nodo;
                    break;
                case "TIPO":
                    TipoMetodo = Tipos_Y_Id(Nodo.ChildNodes[0]);
                    break;
                case "Id":
                    IdMetodo = Tipos_Y_Id(Nodo);
                    break;
            }
        }

        public static String Tipos_Y_Id(ParseTreeNode Nodo)
        {
            String Respuesta = "";
            switch (Nodo.Term.Name.ToString())
            {
                case "Int":
                case "String":
                case "Boolean":
                case "Char":
                case "Double":
                case "Id":
                    Respuesta = Nodo.Token.Value.ToString();
                    break;
            }

            return Respuesta;
        }

        public static void DeclaracionVariables(ParseTreeNode Nodo, List<Simbolos> TabSimbolos)
        {
            switch (Nodo.Term.Name.ToString())
            {
                case "DECLARACIONVARIABLES":
                    IdVariable = Tipos_Y_Id(Nodo.ChildNodes[0]);
                    listaids.Add(IdVariable);

                    DeclaracionVariables(Nodo.ChildNodes[1], TabSimbolos);

                    break;
                case "MASASIG":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        DeclaracionVariables(Nodo.ChildNodes[i], TabSimbolos);
                    }
                    break;
                case "TIPO":
                    TipoVariable = Tipos_Y_Id(Nodo.ChildNodes[0]);
                    break;
                case "ASIGNACION":
                    if (Nodo.ChildNodes.Count == 2)
                    {
                        Valovariable = Condiciones(Nodo.ChildNodes[1]);
                    }
                    break;
                case "LISTAIDS":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        DeclaracionVariables(Nodo.ChildNodes[i], TabSimbolos);
                    }
                    break;

                case "Id":
                    IdVariable = Tipos_Y_Id(Nodo);
                    listaids.Add(IdVariable);
                    break;

                case ";": //se hace la insercion de la variable

                    if (listaids.Count > 1)
                    {
                        for (int i = 0; i < listaids.Count - 1; i++)
                        {
                            switch (TipoVariable)
                            {
                                case "Int":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "0", TipoVariable));
                                    break;
                                case "Boolean":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "false", TipoVariable));
                                    break;
                                case "Double":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "0.0", TipoVariable));
                                    break;
                                case "String":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "\"\"", TipoVariable));
                                    break;
                                case "Char":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "'\u0000'", TipoVariable));
                                    break;
                            }
                        }
                    }
                    if (Valovariable.Equals(""))
                    {
                        switch (TipoVariable)
                        {
                            case "Int":
                                Valovariable = "0";
                                break;
                            case "Boolean":
                                Valovariable = "false";
                                break;
                            case "Double":
                                Valovariable = "0.0";
                                break;
                            case "String":
                                Valovariable = "\"\"";
                                break;
                            case "Char":
                                Valovariable = "'\u0000'";
                                break;
                        }
                    }

                    TabSimbolos.Add(new Simbolos(IdVariable, AmbitoVariable, Nivel, Valovariable, TipoVariable));
                    //Console.WriteLine(IdVariable + ":" + TipoVariable + "=" + Valovariable + "; -> Nivel: " + Nivel);
                    IdVariable = "";
                    TipoVariable = "";
                    Valovariable = "";
                    listaids.Clear();
                    break;
            }
        }

        public static String Operaciones(ParseTreeNode Nodo)
        {
            String resultado = "";
            try
            {
                double number1 = 0.0;
                double number2 = 0.0;

                switch (Nodo.Term.Name.ToString())
                {
                    case "OPERACION":
                        // OPERACIONES = OPERACIONES + OPEREACIONES1 | OPERACIONES1
                        if (Nodo.ChildNodes.Count == 3)
                        {
                            for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                            {
                                if (i == 0)
                                {
                                    number1 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                                else if (i == 2)
                                {
                                    number2 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                            }
                            resultado = (number1 + number2).ToString();
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);

                        }
                        break;
                    case "OPERACIONES1":
                        // OPERACIONES1 = OPERACIONES1 - OPEREACIONES2 | OP0ERACIONES2
                        if (Nodo.ChildNodes.Count == 3)
                        {
                            for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                            {
                                if (i == 0)
                                {
                                    number1 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                                else if (i == 2)
                                {
                                    number2 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                            }
                            resultado = (number1 - number2).ToString();
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);

                        }
                        break;
                    case "OPERACIONES2":
                        // OPERACIONES2 = OPERACIONES2 * OPEREACIONES3 | OPERACIONES3
                        if (Nodo.ChildNodes.Count == 3)
                        {
                            for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                            {
                                if (i == 0)
                                {
                                    number1 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                                else if (i == 2)
                                {
                                    number2 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                            }
                            resultado = (number1 * number2).ToString();
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);

                        }
                        break;
                    case "OPERACIONES3":
                        // OPERACIONES3 = OPERACIONES3 / OPEREACIONES4 | OPERACIONES4
                        if (Nodo.ChildNodes.Count == 3)
                        {
                            for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                            {
                                if (i == 0)
                                {
                                    number1 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                                else if (i == 2)
                                {
                                    number2 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                            }
                            resultado = number1 / number2 + "";
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);

                        }
                        break;
                    case "OPERACIONES4":
                        // OPERACIONES4 = OPERACIONES4 % OPEREACIONES5 | OPERACIONES5
                        if (Nodo.ChildNodes.Count == 3)
                        {
                            for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                            {
                                if (i == 0)
                                {
                                    number1 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                                else if (i == 2)
                                {
                                    number2 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                            }
                            resultado = (number1 % number2) + "";
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);

                        }
                        break;
                    case "OPERACIONES5":
                        // OPERACIONES5 = OPERACIONES5 ^ OPEREACIONES6 | OPERACIONEES6
                        if (Nodo.ChildNodes.Count == 3)
                        {
                            for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                            {
                                if (i == 0)
                                {
                                    number1 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                                else if (i == 2)
                                {
                                    number2 = Convert.ToDouble(Operaciones(Nodo.ChildNodes[i]));
                                }
                            }
                            resultado = Math.Pow(number1, number2) + "";
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);

                        }
                        break;
                    case "OPERACIONES6":
                        //OPERACIONES6 = - OPEREACIONES7 | OPERACIONES7
                        if (Nodo.ChildNodes.Count == 2)
                        {
                            resultado = "-" + Operaciones(Nodo.ChildNodes[1]);
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);
                        }
                        break;
                    case "OPERACIONES7":
                        /*OPERACIONES7.Rule = Entero
                               | Decimal
                               | Id
                               | Id + tkCORA + OPERACION + tkCORC       //acceso a una posicion de un arreglo
                               | Cadena
                               | LLAMADA                                //llamada a un metodo
                               | tkGETUSER + tkPARA + tkPARC            //funcion privada  que devuelde el usuario logeado
                               | tkPARA + OPERACION + tkPARC*/
                        if (Nodo.ChildNodes.Count == 3)     //vienen parentesis entonces por precedencia se hara primero
                        {
                            resultado = Operaciones(Nodo.ChildNodes[1]);
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);
                        }
                        break;

                    case "Entero":
                    case "Decimal":
                    case "Cadena":
                    case "true":
                    case "false":
                        resultado = Nodo.Token.Value.ToString();
                        break;

                    case "Id":      //verificacion que el id exista y tenga valor

                        for (int i = 0; i < Amb.Count; i++)
                        {
                            Ambitos simb = (Ambitos)Amb[i];
                            if (simb.getNivel() == Nivel || simb.getNivel() == 0)
                            {
                                List<Simbolos> si = simb.getSimbolos();
                                for (int j = 0; j < si.Count; j++)
                                {
                                    Simbolos yeah = (Simbolos)si[j];
                                    if (yeah.GetId().Equals(Nodo.Token.Value.ToString()) && (yeah.Getnivel() == Nivel || yeah.Getnivel() == 0))
                                    {
                                        resultado = yeah.GetValor();

                                    }
                                }
                            }
                        }
                        break;
                }
                return resultado;
            }
            catch (Exception e)
            {
                resultado = "Error Semantico de tipo de datos.";
                return resultado;
            }
        }

        public static String Condiciones(ParseTreeNode Nodo)
        {
            String resultado = "";
            String Condicion1 = "";
            String Condicion2 = "";

            switch (Nodo.Term.Name.ToString())
            {
                case "CONDICIONES":
                    /*CONDICIONES.Rule = CONDICIONES + tkIGUAL + tkIGUAL + CONDICIONES1*/
                    if (Nodo.ChildNodes.Count == 4)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Condicion1.Equals(Condicion2)) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    /* CONDICIONES =  CONDICIONES1*/
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES1":
                    /* CONDICIONES1.Rule = CONDICIONES1 + tkDISTINTO + tkIGUAL + CONDICIONES2*/
                    if (Nodo.ChildNodes.Count == 4)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (!Condicion1.Equals(Condicion2)) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES2":
                    /* CONDICIONES2.Rule = CONDICIONES2 + tkMAYORIGUAL + CONDICIONES3*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Convert.ToDouble(Condicion1) >= Convert.ToDouble(Condicion2)) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES3":
                    /*CONDICIONES3.Rule = CONDICIONES3 + tkMENORIGUAL + CONDICIONES4*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Convert.ToDouble(Condicion1) <= Convert.ToDouble(Condicion2)) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES4":
                    /*CONDICIONES4.Rule = CONDICIONES4 + tkMAYOR + CONDICIONES5*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Convert.ToDouble(Condicion1) > Convert.ToDouble(Condicion2)) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;

                case "CONDICIONES5":
                    /* CONDICIONES5.Rule = CONDICIONES5 + tkMENOR + CONDICIONES6*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Convert.ToDouble(Condicion1) < Convert.ToDouble(Condicion2)) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;

                case "CONDICIONES6":
                    /* CONDICIONES6.Rule = CONDICIONES6 + tkOR + CONDICIONES7*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Condicion1.Equals("true") || Condicion2.Equals("true")) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES7":
                    /*CONDICIONES7.Rule = CONDICIONES7 + tkXOR + CONDICIONES8*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Condicion1.Equals("true") && Condicion2.Equals("false"))
                        {
                            resultado = "true";

                        }
                        else if (Condicion1.Equals("false") && Condicion2.Equals("true"))
                        {
                            resultado = "true";
                        }
                        else { resultado = "false"; }

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;

                case "CONDICIONES8":
                    /*CONDICIONES8.Rule = CONDICIONES8 + tkAND + CONDICIONES9*/
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 0)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                            else
                            {
                                Condicion2 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Condicion1.Equals("true") && Condicion2.Equals("true")) resultado = "true";
                        else resultado = "false";

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }
                    break;

                case "CONDICIONES9":
                    /*CONDICIONES9.Rule = tkDISTINTO + CONDICIONES10  <-- ese distitinto funcionara como not*/
                    if (Nodo.ChildNodes.Count == 2)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 1)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Condicion1.Equals("true")) resultado = "false";
                        else resultado = "true";


                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }
                    break;

                case "CONDICIONES10":
                    /* CONDICIONES10.Rule = OPERACION
                                | tkTRUE
                                | tkFALSE
                                | Id + tkPUNTO + tkCOMPARE + tkPARA + Cadena + tkPARC           //compare porque siempre devolvera un tru o un false
                                | tkPARA + CONDICIONES + tkPARC*/

                    if (Nodo.ChildNodes.Count == 1)
                    {
                        if (Nodo.ChildNodes[0].Term.Name.ToString().Equals("OPERACION"))
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);
                        }
                        else
                        {
                            resultado = Nodo.ChildNodes[0].Token.Value.ToString();
                        }
                    }
                    else if (Nodo.ChildNodes.Count == 3)
                    {
                        resultado = Condiciones(Nodo.ChildNodes[1]);
                    }
                    else if (Nodo.ChildNodes.Count == 6)
                    {
                        Console.WriteLine("Entro a Compareto.");
                    }
                    break;

                case "Id":      //verificacion que el id exista y tenga valor
                    for (int i = 0; i < Amb.Count; i++)
                    {
                        Ambitos simb = (Ambitos)Amb[i];
                        if (simb.getNivel() == Nivel)
                        {
                            List<Simbolos> si = simb.getSimbolos();
                            for (int j = 0; j < si.Count; j++)
                            {
                                Simbolos yeah = (Simbolos)si[j];
                                if (yeah.GetId().Equals(Nodo.Token.Value.ToString()) && yeah.Getnivel() == Nivel)
                                {
                                    resultado = yeah.GetValor();
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            return resultado;
        }

    }
}
