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
        static String tipoparentesis = "";
        static Boolean Yasedetubo = false;
        static Boolean Break = false;
        static String NombreMetodoActual = "";
        static Retorno Returnmetodo = new Retorno("", "");

        static int Nivel = 0;
        static int contadorambitos = 0;
        static String Print = "";

        //arreglos
        static String IdArreglo = "";
        static String TipoArreglo = "";
        static int TamanioArreglo = 0;
        static List<String> Valores = new List<string>();

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
        public static List<String> ErroresSemanticos = new List<string>();
        static List<MetodosYFuncion> Tablafunciones = new List<MetodosYFuncion>();      //en la primera pasada almacenaremos las funciones
        static List<Simbolos> TablaSimbolos = new List<Simbolos>();                     //lista de variables que vayamos reconociendo
        static List<Ambitos> Amb = new List<Ambitos>();                                 //lista que funcionara como pila de ambitos

        public static void RealizarAccionesAcciones(ParseTreeNode Raiz)
        {

            #region LIMPIEZA DE VARIABLE
            tipoparentesis = "";

            NombreMetodoActual = "";
            Returnmetodo = null;
            Yasedetubo = false; //sirve para detener cuando venga un return o break
            Break = false;

            Nivel = 0;
            contadorambitos = 0;
            Print = "";

            TamanioArreglo = 0;
            IdArreglo = "";
            TipoArreglo = "";
            Valores.Clear();

            IdVariable = "";
            TipoVariable = "";
            Valovariable = "";
            AmbitoVariable = "Global";

            listaparametros.Clear();
            Impresiones.Clear();
            ErroresSemanticos.Clear();
            TablaSimbolos.Clear();
            listaids.Clear();
            Amb.Clear();
            Tablafunciones.Clear();

            #endregion

            PrimeraPasada(Raiz);            //reconocimiento de variables globales y metodos y funciones

            //recorremos los metodos guardados hasta encontrar el main
            for (int i = 0; i < Tablafunciones.Count; i++)  //Buscamos el metodo main para empezar la ejecucion de codigo.
            {
                MetodosYFuncion metodo = (MetodosYFuncion)Tablafunciones[i];
                if (metodo.getNombre().Equals("Main") || metodo.getNombre().Equals("main"))
                {
                    AmbitoVariable = "Main";
                    NombreMetodoActual = AmbitoVariable;
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
            Yasedetubo = false;
            // Break = false;
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
                            if (Nodo.ChildNodes[i].ChildNodes.Count == 3)
                            {
                                String id = Tipos_Y_Id(Nodo.ChildNodes[i].ChildNodes[0]);

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
                                                if (Nodo.ChildNodes[i].ChildNodes[1].ChildNodes[0].Term.Name.ToString().Equals("+"))
                                                {
                                                    yeah.SetValor((Convert.ToDouble(yeah.GetValor()) + 1).ToString());
                                                }
                                                else
                                                {
                                                    yeah.SetValor((Convert.ToDouble(yeah.GetValor()) - 1).ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                String id = Tipos_Y_Id(Nodo.ChildNodes[i].ChildNodes[0]);
                                Retorno valor = Condiciones(Nodo.ChildNodes[i].ChildNodes[2]);

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
                                                yeah.SetValor(valor.getValor());
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else if (Nodo.ChildNodes[i].Term.Name.ToString().Equals("DECLARACIONARREGLOS"))
                        {
                            for (int z = 0; z < Amb.Count; z++)
                            {
                                Ambitos amb = (Ambitos)Amb[z];
                                if (amb.getAmbito().Equals("Global") && amb.getNivel() == Nivel)
                                {
                                    DeclaracionArreglos(Nodo.ChildNodes[i], amb.getSimbolos());
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
            if (!Yasedetubo && !Break)  //que no haya return o break
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
                                    sim.Add(new Simbolos(Idparametro, Ambito, Nivel, ValorParametro, Tipoparametro, false));
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
                            if (Nodo.ChildNodes[0].Term.Name.ToString().Equals("Return"))
                            {
                                for (int i = 0; i < Tablafunciones.Count; i++)
                                {
                                    MetodosYFuncion metodo = (MetodosYFuncion)Tablafunciones[i];
                                    if (metodo.getNombre().Equals(NombreMetodoActual))
                                    {
                                        metodo.setReturn(Nodo);
                                        Tablafunciones[i] = metodo; //actualizamos el return y actualizamos la funcion o metodo establecido
                                        Yasedetubo = true;
                                        break;
                                    }
                                }
                            }
                            else if (Nodo.ChildNodes[0].Term.Name.ToString().Equals("Break"))
                            {
                                Break = true;
                            }
                            else
                            {
                                EjecucionCodigo(Nodo.ChildNodes[0], Ambito, Nivel);
                            }
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
                    case "DECLARACIONARREGLOS":
                        for (int i = 0; i < Amb.Count; i++)
                        {
                            Ambitos amb = (Ambitos)Amb[i];
                            if (amb.getAmbito().Equals(Ambito) && amb.getNivel() == Nivel)
                            {
                                DeclaracionArreglos(Nodo, amb.getSimbolos());
                            }
                        }
                        break;
                    case "ASIGNACIONVAR":
                        if (Nodo.ChildNodes.Count == 3)
                        {
                            String id = Tipos_Y_Id(Nodo.ChildNodes[0]);

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
                                            if (Nodo.ChildNodes[1].ChildNodes[0].Term.Name.ToString().Equals("+"))
                                            {
                                                yeah.SetValor((Convert.ToDouble(yeah.GetValor()) + 1).ToString());
                                            }
                                            else
                                            {
                                                yeah.SetValor((Convert.ToDouble(yeah.GetValor()) - 1).ToString());
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            String id = Tipos_Y_Id(Nodo.ChildNodes[0]);
                            Retorno valor = Condiciones(Nodo.ChildNodes[2]);

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
                                            yeah.SetValor(valor.getValor());
                                        }
                                    }
                                }
                            }
                        }

                        break;

                    #region LLAMADA
                    case "LLAMADA":
                        // LLamadaMetodo(Nodo, Ambito, Niv);
                        String AmbitoAnterior100 = AmbitoVariable;

                        String Idmetodollamar = Tipos_Y_Id(Nodo.ChildNodes[0]);
                        EjecucionCodigo(Nodo.ChildNodes[2], Ambito, Nivel);     //mandamos a actualizar el dato de sus parametros. y luego las acciones

                        for (int i = 0; i < Tablafunciones.Count; i++)  //Buscamos el metodo main para empezar la ejecucion de codigo.
                        {
                            MetodosYFuncion metodo = (MetodosYFuncion)Tablafunciones[i];
                            if (metodo.getNombre().Equals(Idmetodollamar))
                            {
                                String MetodoAnterior = NombreMetodoActual; //capturamos el metodo anterior
                                AmbitoVariable = Idmetodollamar + contadorambitos;
                                NombreMetodoActual = Idmetodollamar;

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

                                EjecucionCodigo(metodo.getCuerpo(), AmbitoVariable, Nivel); //ejecutamos el cuerpo del metodo

                                if (metodo.getReturn() != null) Returnmetodo = Operaciones(metodo.getReturn().ChildNodes[1]);   //capturamos su valor del return simepre y cuando no sea nul;

                                DesapilarAmbito(AmbitoVariable, Nivel);
                                AmbitoVariable = AmbitoAnterior100;
                                Niv--;
                                Nivel = Niv;
                                contadorambitos--;
                                NombreMetodoActual = MetodoAnterior;
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

                        Retorno para = Condiciones(Nodo.ChildNodes[0]);
                        listaparametros.Add(para.getValor());

                        break;
                    // break;
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
                            Retorno ret = Operaciones(Nodo.ChildNodes[2]);
                            Print += ret.getValor();
                        }
                        else
                        {
                            Retorno ret1 = Operaciones(Nodo.ChildNodes[0]);
                            Print += ret1.getValor();
                        }
                        break;

                    case "OPERACION":
                        Retorno ret2 = Operaciones(Nodo);
                        Print += ret2.getValor();
                        break;
                    #endregion

                    #region IF-ELSE
                    case "IF":
                        String AmbitoAnterior = AmbitoVariable;
                        AmbitoVariable = "IF" + contadorambitos;
                        contadorambitos++;
                        Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                        Retorno condi = Condiciones(Nodo.ChildNodes[2]);         //si condiciones es true se mete al if  de lo contrario se mete al else

                        if (Convert.ToBoolean(condi.getValor()))       //condicion true
                        {
                            EjecucionCodigo(Nodo.ChildNodes[5], AmbitoVariable, Nivel);
                        }
                        else                               //condicion false
                        {
                            if (Nodo.ChildNodes.Count == 8)          //esto debido a que el bloque else puede o no venir 
                            {//si existe bloque else que entre

                                EjecucionCodigo(Nodo.ChildNodes[7], AmbitoVariable, Nivel);
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

                            EjecucionCodigo(Nodo.ChildNodes[2], AmbitoVariable, Nivel);

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

                        Retorno condi1 = null;
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (!Break)
                            {
                                if (i == 2)
                                {
                                    condi1 = Condiciones(Nodo.ChildNodes[2]);         //si condiciones es true se mete al if  de lo contrario se mete al else

                                    if (!Convert.ToBoolean(condi1.getValor()))       //condicion true
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (i == 5 && Convert.ToBoolean(condi1.getValor()))
                                    {
                                        while (Convert.ToBoolean(condi1.getValor()))
                                        {
                                            EjecucionCodigo(Nodo.ChildNodes[i], AmbitoVariable, Nivel);
                                            EjecucionCodigo(Nodo, AmbitoVariable, Nivel);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Break = false;
                                break;
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

                        Retorno condi2 = null;
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (!Break)
                            {
                                if (i == 6)
                                {
                                    condi2 = Condiciones(Nodo.ChildNodes[6]);         //si condiciones es true se mete al if  de lo contrario se mete al else

                                    if (Convert.ToBoolean(condi2.getValor()))       //condicion true
                                    {
                                        EjecucionCodigo(Nodo, AmbitoVariable, Nivel);
                                    }
                                }
                                else
                                {
                                    EjecucionCodigo(Nodo.ChildNodes[i], AmbitoVariable, Nivel);
                                }
                            }
                            else
                            {
                                Break = false;
                                break;
                            }
                        }

                        DesapilarAmbito(AmbitoVariable, Nivel);      //desapilamos el ambito de la pila de ambitos
                        contadorambitos--;
                        AmbitoVariable = AmbitoAnterior3;            //regreso al ambito donde se encontraba
                        AmbitoAnterior = "";
                        break;
                    #endregion

                    #region FOR 
                    case "FOR":

                        String Variabledefor = "";
                        //FOR.Rule = tkFOR + tkPARA + DECLARACIONVARIABLES + CONDICIONES + tkPUNTOYCOMA + Id + INCREODECRE + tkPARC + tkLLAVA + LISTASENTENCIAS + tkLLAVC;
                        String AmbitoAnterior4 = AmbitoVariable;
                        AmbitoVariable = "FOR" + contadorambitos;
                        contadorambitos++;
                        Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                        bool Increodecre = false;
                        Retorno condi3 = null;
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (!Break)
                            {
                                if (i == 2)
                                {
                                    Variabledefor = Nodo.ChildNodes[2].ChildNodes[0].Token.Value.ToString();
                                    EjecucionCodigo(Nodo.ChildNodes[2], AmbitoVariable, Nivel);
                                }
                                else if (i == 3)
                                {
                                    condi3 = Condiciones(Nodo.ChildNodes[3]);         //si condiciones es true se mete al if  de lo contrario se mete al else

                                    if (Convert.ToBoolean(condi3.getValor()))       //condicion true
                                    {
                                        EjecucionCodigo(Nodo.ChildNodes[9], AmbitoVariable, Nivel);
                                        i = 4;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (i == 6)
                                    {
                                        if (Nodo.ChildNodes[i].ChildNodes[0].Term.Name.ToString().Equals("+"))
                                        {
                                            Increodecre = true;
                                        }
                                        else
                                        {
                                            Increodecre = false;
                                        }
                                    }
                                    else if (i == 7)
                                    {
                                        for (int x = 0; x < Amb.Count; x++)
                                        {
                                            Ambitos simb = (Ambitos)Amb[x];
                                            if (simb.getNivel() == Nivel && simb.getAmbito().Equals(AmbitoVariable))
                                            {
                                                List<Simbolos> si = simb.getSimbolos();
                                                for (int l = 0; l < si.Count; l++)
                                                {
                                                    Simbolos yeah = (Simbolos)si[l];
                                                    if (yeah.GetId().Equals(Variabledefor))
                                                    {
                                                        if (Increodecre)
                                                        {
                                                            yeah.SetValor((Convert.ToInt32(yeah.GetValor()) + 1).ToString());
                                                        }
                                                        else
                                                        {
                                                            yeah.SetValor((Convert.ToInt32(yeah.GetValor()) - 1).ToString());

                                                        }
                                                        break;
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        i = 2;
                                    }
                                }
                            }
                            else
                            {
                                Break = false;
                                break;
                            }
                        }

                        DesapilarAmbito(AmbitoVariable, Nivel);      //desapilamos el ambito de la pila de ambitos
                        contadorambitos--;
                        AmbitoVariable = AmbitoAnterior4;            //regreso al ambito donde se encontraba
                        AmbitoAnterior = "";
                        break;
                    #endregion

                    #region SWITCH
                    case "SWITCH":
                        //   SWITCH.Rule = tkSWITCH + tkPARA + OPERACION + tkPARC + tkLLAVA + LISTACASOS + DEFAULT + tkLLAVC;
                        String AmbitoAnterior5 = AmbitoVariable;
                        AmbitoVariable = "SWITCH" + contadorambitos;
                        contadorambitos++;
                        Amb.Add(new Ambitos(AmbitoVariable, new List<Simbolos>(), Nivel));      //Creamos un nuevo ambito

                        Retorno OperecionSwitch = Operaciones(Nodo.ChildNodes[2]);
                        Boolean Entrocasos = Switch(Nodo.ChildNodes[5], OperecionSwitch.getValor());

                        if (!Entrocasos) { Switch(Nodo.ChildNodes[6], ""); }         //si no entro en ninguna caso mando el nodo default

                        DesapilarAmbito(AmbitoVariable, Nivel);      //desapilamos el ambito de la pila de ambitos
                        contadorambitos--;
                        AmbitoVariable = AmbitoAnterior5;            //regreso al ambito donde se encontraba
                        AmbitoAnterior = "";
                        break;
                        #endregion

                }
            }
        }

        public static Boolean Switch(ParseTreeNode Nodo, String Operacion)
        {
            Boolean entrocasos = false;
            switch (Nodo.Term.Name.ToString())
            {
                case "LISTACASOS":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        Switch(Nodo.ChildNodes[i], Operacion);
                    }
                    break;
                case "CASOS":
                    //tkCASE + OPERACION + tkDOSPUNTOS + LISTASENTENCIAS + tkBREAK + tkPUNTOYCOMA;
                    Retorno Operacionaevaluar = Operaciones(Nodo.ChildNodes[1]);
                    if (Operacionaevaluar.getValor().Equals(Operacion))
                    {
                        entrocasos = true;
                        EjecucionCodigo(Nodo.ChildNodes[3], AmbitoVariable, Nivel);
                        return entrocasos;
                    }

                    break;
                case "DEFAULT":
                    //   DEFAULT.Rule = tkDEFAULT + tkDOSPUNTOS + LISTASENTENCIAS + tkBREAK + tkPUNTOYCOMA

                    if (Nodo.ChildNodes.Count > 0)      //verifico que el nodo no sea empty
                    {
                        EjecucionCodigo(Nodo.ChildNodes[2], AmbitoVariable, Nivel);
                    }
                    break;
            }

            return entrocasos;
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

        public static void DeclaracionArreglos(ParseTreeNode Nodo, List<Simbolos> TabSimbolos)
        {
            switch (Nodo.Term.Name.ToString())
            {
                case "DECLARACIONARREGLOS":
                    IdArreglo = Tipos_Y_Id(Nodo.ChildNodes[0]);
                    TipoArreglo = Tipos_Y_Id(Nodo.ChildNodes[2]);
                    TamanioArreglo = Convert.ToInt32(Operaciones(Nodo.ChildNodes[4]));

                    DeclaracionArreglos(Nodo.ChildNodes[6], TabSimbolos);    //captura si existe la inicializacion.

                    String[] valo = new string[TamanioArreglo];
                    if (Valores.Count > 0)
                    {

                        for (int i = 0; i < Valores.Count; i++)
                        {
                            valo[i] = Valores[i];
                        }

                        TabSimbolos.Add(new Simbolos(IdArreglo, AmbitoVariable, Nivel, valo, TipoMetodo, true));

                    }
                    else
                    {
                        TabSimbolos.Add(new Simbolos(IdArreglo, AmbitoVariable, Nivel, valo, TipoMetodo, true));
                    }

                    IdArreglo = "";
                    TipoArreglo = ";";
                    TamanioArreglo = 0;
                    Valores.Clear();

                    break;
                case "INICIALIZACIONARREGLO":
                    if (Nodo.ChildNodes.Count > 0)
                    {
                        DeclaracionArreglos(Nodo.ChildNodes[2], TabSimbolos);
                    }
                    break;

                case "LISTADATOS":
                    for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                    {
                        DeclaracionArreglos(Nodo.ChildNodes[i], TabSimbolos);
                    }
                    break;
                case "CONDICIONES":
                    Retorno valor = Condiciones(Nodo);
                    Valores.Add(valor.getValor());
                    break;
            }
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
                        Retorno reto = Condiciones(Nodo.ChildNodes[1]);
                        Valovariable = reto.getValor();
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
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "0", TipoVariable, false));
                                    break;
                                case "Boolean":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "false", TipoVariable, false));
                                    break;
                                case "Double":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "0.0", TipoVariable, false));
                                    break;
                                case "String":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "\"\"", TipoVariable, false));
                                    break;
                                case "Char":
                                    TabSimbolos.Add(new Simbolos(listaids[i], AmbitoVariable, Nivel, "'\u0000'", TipoVariable, false));
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

                    TabSimbolos.Add(new Simbolos(IdVariable, AmbitoVariable, Nivel, Valovariable, TipoVariable, false));
                    //Console.WriteLine(IdVariable + ":" + TipoVariable + "=" + Valovariable + "; -> Nivel: " + Nivel);
                    IdVariable = "";
                    TipoVariable = "";
                    Valovariable = "";
                    listaids.Clear();
                    break;
            }
        }

        public static Retorno Condiciones(ParseTreeNode Nodo)
        {
            Retorno resultado = new Retorno("", "");
            Retorno Condicion1 = null;
            Retorno Condicion2 = null;

            switch (Nodo.Term.Name.ToString())
            {
                case "CONDICIONES":
                    //CONDICIONES.Rule = CONDICIONES + tkIGUAL + tkIGUAL + CONDICIONES1
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

                        if (Condicion1.getValor().Equals(Condicion2.getValor())) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    // CONDICIONES =  CONDICIONES1
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES1":
                    // CONDICIONES1.Rule = CONDICIONES1 + tkDISTINTO + tkIGUAL + CONDICIONES2
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

                        if (!Condicion1.getValor().Equals(Condicion2.getValor())) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES2":
                    // CONDICIONES2.Rule = CONDICIONES2 + tkMAYORIGUAL + CONDICIONES3
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

                        if (Convert.ToDouble(Condicion1.getValor()) >= Convert.ToDouble(Condicion2.getValor())) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES3":
                    //CONDICIONES3.Rule = CONDICIONES3 + tkMENORIGUAL + CONDICIONES4
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

                        if (Convert.ToDouble(Condicion1.getValor()) <= Convert.ToDouble(Condicion2.getValor())) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES4":
                    //CONDICIONES4.Rule = CONDICIONES4 + tkMAYOR + CONDICIONES5
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

                        if (Convert.ToDouble(Condicion1.getValor()) > Convert.ToDouble(Condicion2.getValor())) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;

                case "CONDICIONES5":
                    // CONDICIONES5.Rule = CONDICIONES5 + tkMENOR + CONDICIONES6
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

                        if (Convert.ToDouble(Condicion1.getValor()) < Convert.ToDouble(Condicion2.getValor())) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;

                case "CONDICIONES6":
                    // CONDICIONES6.Rule = CONDICIONES6 + tkOR + CONDICIONES7
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

                        if (Condicion1.getValor().Equals("true") || Condicion2.getValor().Equals("true")) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;
                case "CONDICIONES7":
                    //CONDICIONES7.Rule = CONDICIONES7 + tkXOR + CONDICIONES8
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

                        if (Condicion1.getValor().Equals("true") && Condicion2.getValor().Equals("false"))
                        {
                            resultado.setValor("true");

                        }
                        else if (Condicion1.getValor().Equals("false") && Condicion2.getValor().Equals("true"))
                        {
                            resultado.setValor("true");
                        }
                        else { resultado.setValor("false"); }

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }

                    break;

                case "CONDICIONES8":
                    //CONDICIONES8.Rule = CONDICIONES8 + tkAND + CONDICIONES9
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

                        if (Condicion1.getValor().Equals("true") && Condicion2.getValor().Equals("true")) resultado.setValor("true");
                        else resultado.setValor("false");

                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }
                    break;

                case "CONDICIONES9":
                    //CONDICIONES9.Rule = tkDISTINTO + CONDICIONES10  <-- ese distitinto funcionara como not
                    if (Nodo.ChildNodes.Count == 2)
                    {
                        for (int i = 0; i < Nodo.ChildNodes.Count; i++)
                        {
                            if (i == 1)
                            {
                                Condicion1 = Condiciones(Nodo.ChildNodes[i]);
                            }
                        }

                        if (Condicion1.getValor().Equals("true")) resultado.setValor("false");
                        else resultado.setValor("true");


                        return resultado;
                    }
                    else
                    {
                        resultado = Condiciones(Nodo.ChildNodes[0]);
                    }
                    break;

                case "CONDICIONES10":
                    /* CONDICIONES10.Rule = OPERACION
                             
                                | Id + tkPUNTO + tkCOMPARE + tkPARA + Cadena + tkPARC           //compare porque siempre devolvera un tru o un false
                                | tkPARA + CONDICIONES + tkPARC*/

                    if (Nodo.ChildNodes.Count == 1)
                    {
                        if (Nodo.ChildNodes[0].Term.Name.ToString().Equals("OPERACION"))
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);
                        }
                    }
                    else if (Nodo.ChildNodes.Count == 3)
                    {
                        resultado = Condiciones(Nodo.ChildNodes[1]);
                    }
                    else if (Nodo.ChildNodes.Count == 6)
                    {
                        Retorno variable = Operaciones(Nodo.ChildNodes[0]);      //capturo su valor
                        String cadena = Nodo.ChildNodes[4].Token.Value.ToString();

                        if (variable.getValor().Equals(cadena)) resultado.setValor("true");
                        else resultado.setValor("false");

                        //Console.WriteLine("Entro a Compareto.");
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
                                    resultado.setValor(yeah.GetValor());
                                    resultado.setTipo(yeah.GetTipo());
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

        public static Retorno operacionSeguntipos(String tipodato1, String tipodato2, String number1, String number2, String Operacion)
        {
            Retorno resultado = new Retorno("", "");

            if (Operacion.Equals("suma"))
            {
                #region SUMA
                if (tipodato1.Equals("boolean") && tipodato2.Equals("boolean"))
                {
                    if (Convert.ToBoolean(number1) && Convert.ToBoolean(number2))
                    {
                        resultado.setValor("2.0");
                    }
                    else if (!Convert.ToBoolean(number1) && Convert.ToBoolean(number2))
                    {
                        resultado.setValor("1.0");
                    }
                    else if (Convert.ToBoolean(number1) && !Convert.ToBoolean(number2))
                    {
                        resultado.setValor("2.0");
                    }
                    else if (!Convert.ToBoolean(number1) && !Convert.ToBoolean(number2))
                    {
                        resultado.setValor("0.0");
                    }
                    resultado.setTipo("double");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("double"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 + Convert.ToDouble(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 + Convert.ToDouble(number2)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("double"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 + Convert.ToDouble(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 + Convert.ToDouble(number1)).ToString());
                    }
                    resultado.setTipo("double");
                }


                else if ((tipodato1.Equals("boolean") && tipodato2.Equals("string")) || (tipodato2.Equals("boolean") && tipodato1.Equals("string")))
                {
                    resultado.setValor(number1 + number2);
                    resultado.setTipo("string");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("int"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 + Convert.ToInt32(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 + Convert.ToInt32(number2)).ToString());
                    }
                    resultado.setTipo("int");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("int"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 + Convert.ToInt32(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 + Convert.ToInt32(number1)).ToString());
                    }
                    resultado.setTipo("int");
                }
                else if ((tipodato2.Equals("boolean") && tipodato1.Equals("char")) || (tipodato1.Equals("boolean") && tipodato2.Equals("char")))
                {
                    resultado.setValor("Error tipo dato al operar Boolean con Char.");
                    resultado.setTipo("Error tipo dato al operar Boolean con Char.");
                }


                //doubles 

                else if ((tipodato2.Equals("double") && tipodato1.Equals("double")) || (tipodato1.Equals("double") && tipodato2.Equals("double")))
                {
                    resultado.setValor((Convert.ToDouble(number1) + Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if ((tipodato1.Equals("double") && tipodato2.Equals("string")) || (tipodato2.Equals("double") && tipodato1.Equals("string")))
                {
                    resultado.setValor(number1 + number2);
                    resultado.setTipo("string");
                }

                else if (tipodato1.Equals("double") && tipodato2.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) + Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) + Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato1.Equals("double") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number1) + Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number2) + Encoding.ASCII.GetBytes(number1)[0]).ToString());
                    resultado.setTipo("double");

                }

                //ints
                else if ((tipodato1.Equals("int") && tipodato2.Equals("string")) || (tipodato2.Equals("int") && tipodato1.Equals("string")))
                {
                    resultado.setValor(number1 + number2);
                    resultado.setTipo("string");
                }

                else if ((tipodato2.Equals("int") && tipodato1.Equals("int")) || (tipodato1.Equals("int") && tipodato2.Equals("int")))
                {
                    resultado.setValor((Convert.ToInt32(number1) + Convert.ToInt32(number2)).ToString());
                    resultado.setTipo("int");

                }
                else if (tipodato1.Equals("int") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToInt32(number1) + Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("int");

                }
                else if (tipodato2.Equals("int") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Convert.ToInt32(number2) + Encoding.ASCII.GetBytes(number1)[0]).ToString());
                    resultado.setTipo("int");

                }

                //chars 

                else if ((tipodato1.Equals("char") && tipodato2.Equals("string")) || (tipodato2.Equals("char") && tipodato1.Equals("string")))
                {
                    resultado.setValor(number1 + number2);
                    resultado.setTipo("string");
                }

                else if ((tipodato2.Equals("char") && tipodato1.Equals("char")) || (tipodato1.Equals("char") && tipodato2.Equals("char")))
                {
                    resultado.setValor((Convert.ToInt32(Encoding.ASCII.GetBytes(number1)[0]) + Convert.ToInt32(Encoding.ASCII.GetBytes(number2)[0])).ToString());
                    resultado.setTipo("int");

                }

                // string
                else if ((tipodato1.Equals("string") && tipodato2.Equals("string")) || (tipodato2.Equals("string") && tipodato1.Equals("string")))
                {
                    resultado.setValor(number1 + number2);
                    resultado.setTipo("string");
                }
                #endregion}
            }

            else if (Operacion.Equals("resta"))
            {
                #region RESTA
                if (tipodato1.Equals("boolean") && tipodato2.Equals("boolean"))
                {
                    resultado.setValor("Error tipo dato al operar Boolean con Boolean.");
                    resultado.setTipo("Error tipo dato al operar Boolean con Boolean.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("double"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 - Convert.ToDouble(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 - Convert.ToDouble(number2)).ToString());
                    }
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("double"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 - Convert.ToDouble(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 - Convert.ToDouble(number1)).ToString());
                    }
                    resultado.setTipo("double");
                }

                else if ((tipodato1.Equals("boolean") && tipodato2.Equals("string")) || (tipodato2.Equals("boolean") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Boolean con String.");
                    resultado.setTipo("Error tipo dato al operar Boolean con String.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("int"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 - Convert.ToInt32(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 - Convert.ToInt32(number2)).ToString());
                    }
                    resultado.setTipo("int");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("int"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 - Convert.ToInt32(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 - Convert.ToInt32(number1)).ToString());
                    }
                    resultado.setTipo("int");
                }
                else if ((tipodato2.Equals("boolean") && tipodato1.Equals("char")) || (tipodato1.Equals("boolean") && tipodato2.Equals("char")))
                {
                    resultado.setValor("Error tipo dato al operar Boolean con Char.");
                    resultado.setTipo("Error tipo dato al operar Boolean con Char.");
                }

                //doubles 

                else if ((tipodato2.Equals("double") && tipodato1.Equals("double")) || (tipodato1.Equals("double") && tipodato2.Equals("double")))
                {
                    resultado.setValor((Convert.ToDouble(number1) - Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if ((tipodato1.Equals("double") && tipodato2.Equals("string")) || (tipodato2.Equals("double") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Double con String.");
                    resultado.setTipo("Error tipo dato al operar Double con String.");
                }

                else if (tipodato1.Equals("double") && tipodato2.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) - Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) - Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato1.Equals("double") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number1) - Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] - Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }

                //ints
                else if ((tipodato1.Equals("int") && tipodato2.Equals("string")) || (tipodato2.Equals("int") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Int con String.");
                    resultado.setTipo("Error tipo dato al operar Int con String.");
                }

                else if ((tipodato2.Equals("int") && tipodato1.Equals("int")) || (tipodato1.Equals("int") && tipodato2.Equals("int")))
                {
                    resultado.setValor((Convert.ToInt32(number1) - Convert.ToInt32(number2)).ToString());
                    resultado.setTipo("int");

                }
                else if (tipodato1.Equals("int") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToInt32(number1) - Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("int");

                }
                else if (tipodato2.Equals("int") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] - Convert.ToInt32(number2)).ToString());
                    resultado.setTipo("int");

                }

                //chars 

                else if ((tipodato1.Equals("char") && tipodato2.Equals("string")) || (tipodato2.Equals("char") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Char con String.");
                    resultado.setTipo("Error tipo dato al operar Char con String.");
                }

                else if ((tipodato2.Equals("char") && tipodato1.Equals("char")) || (tipodato1.Equals("char") && tipodato2.Equals("char")))
                {
                    resultado.setValor((Convert.ToInt32(Encoding.ASCII.GetBytes(number1)[0]) - Convert.ToInt32(Encoding.ASCII.GetBytes(number2)[0])).ToString());
                    resultado.setTipo("int");
                }

                // string
                else if ((tipodato1.Equals("string") && tipodato2.Equals("string")) || (tipodato2.Equals("string") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar String con String.");
                    resultado.setTipo("Error tipo dato al operar String con String.");
                }
                #endregion
            }
            else if (Operacion.Equals("multiplicacion"))
            {
                #region MULTIPLICAICION
                if (tipodato1.Equals("boolean") && tipodato2.Equals("boolean"))
                {
                    resultado.setValor("Error tipo dato al operar Boolean con Boolean.");
                    resultado.setTipo("Error tipo dato al operar Boolean con Boolean.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("double"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 * Convert.ToDouble(number2)).ToString());
                        resultado.setTipo("double");
                    }
                    else
                    {
                        resultado.setValor((0 * Convert.ToDouble(number2)).ToString());
                        resultado.setTipo("double");
                    }

                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("double"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 * Convert.ToDouble(number1)).ToString());
                        resultado.setTipo("double");
                    }
                    else
                    {
                        resultado.setValor((0 * Convert.ToDouble(number1)).ToString());
                        resultado.setTipo("double");
                    }

                }

                else if ((tipodato1.Equals("boolean") && tipodato2.Equals("string")) || (tipodato2.Equals("boolean") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Boolean con String.");
                    resultado.setTipo("Error tipo dato al operar Boolean con String.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("int"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 * Convert.ToInt32(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 * Convert.ToInt32(number2)).ToString());
                    }
                    resultado.setTipo("int");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("int"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 * Convert.ToInt32(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 * Convert.ToInt32(number1)).ToString());
                    }
                    resultado.setTipo("int");
                }
                else if ((tipodato2.Equals("boolean") && tipodato1.Equals("char")) || (tipodato1.Equals("boolean") && tipodato2.Equals("char")))
                {
                    resultado.setValor("Error tipo dato al operar Boolean con Char.");
                    resultado.setTipo("Error tipo dato al operar Boolean con Char.");
                }


                //doubles 

                else if ((tipodato2.Equals("double") && tipodato1.Equals("double")) || (tipodato1.Equals("double") && tipodato2.Equals("double")))
                {
                    resultado.setValor((Convert.ToDouble(number1) * Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if ((tipodato1.Equals("double") && tipodato2.Equals("string")) || (tipodato2.Equals("double") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Double con String.");
                    resultado.setTipo("Error tipo dato al operar Double con String.");
                }

                else if (tipodato1.Equals("double") && tipodato2.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) * Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) * Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato1.Equals("double") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number1) * Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] * Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }

                //ints
                else if ((tipodato1.Equals("int") && tipodato2.Equals("string")) || (tipodato2.Equals("int") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Int con String.");
                    resultado.setTipo("Error tipo dato al operar Int con String.");
                }

                else if ((tipodato2.Equals("int") && tipodato1.Equals("int")) || (tipodato1.Equals("int") && tipodato2.Equals("int")))
                {
                    resultado.setValor((Convert.ToInt32(number1) * Convert.ToInt32(number2)).ToString());
                    resultado.setTipo("int");

                }
                else if (tipodato1.Equals("int") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToInt32(number1) * Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("int");

                }
                else if (tipodato2.Equals("int") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] * Convert.ToInt32(number2)).ToString());
                    resultado.setTipo("int");

                }

                //chars 

                else if ((tipodato1.Equals("char") && tipodato2.Equals("string")) || (tipodato2.Equals("char") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar Char con String.");
                    resultado.setTipo("Error tipo dato al operar Char con String.");
                }

                else if ((tipodato2.Equals("char") && tipodato1.Equals("char")) || (tipodato1.Equals("char") && tipodato2.Equals("char")))
                {
                    resultado.setValor((Convert.ToInt32(Encoding.ASCII.GetBytes(number1)[0]) * Convert.ToInt32(Encoding.ASCII.GetBytes(number2)[0])).ToString());
                    resultado.setTipo("int");
                }

                // string
                else if ((tipodato1.Equals("string") && tipodato2.Equals("string")) || (tipodato2.Equals("string") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar String con String.");
                    resultado.setTipo("Error tipo dato al operar String con String.");
                }
                #endregion
            }
            else if (Operacion.Equals("division"))
            {
                #region DIVISION
                if (tipodato1.Equals("boolean") && tipodato2.Equals("boolean"))
                {
                    resultado.setValor("Error tipo dato al operar boolean con boolean.");
                    resultado.setTipo("Error tipo dato al operar boolean con boolean.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("double"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 / Convert.ToDouble(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 / Convert.ToDouble(number2)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("double"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 / Convert.ToDouble(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 / Convert.ToDouble(number1)).ToString());
                    }
                    resultado.setTipo("double");
                }

                else if ((tipodato1.Equals("boolean") && tipodato2.Equals("string")) || (tipodato2.Equals("boolean") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar boolean con string.");
                    resultado.setTipo("Error tipo dato al operar boolean con string.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("int"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 / Convert.ToDouble(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 / Convert.ToDouble(number2)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("int"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 / Convert.ToDouble(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 / Convert.ToDouble(number1)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if ((tipodato2.Equals("boolean") && tipodato1.Equals("char")) || (tipodato1.Equals("boolean") && tipodato2.Equals("char")))
                {
                    resultado.setValor("Error tipo dato al operar boolean con char.");
                    resultado.setTipo("Error tipo dato al operar boolean con char.");
                }


                //doubles 

                else if ((tipodato2.Equals("double") && tipodato1.Equals("double")) || (tipodato1.Equals("double") && tipodato2.Equals("double")))
                {
                    resultado.setValor((Convert.ToDouble(number1) / Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if ((tipodato1.Equals("double") && tipodato2.Equals("string")) || (tipodato2.Equals("double") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar double con string.");
                    resultado.setTipo("Error tipo dato al operar double con string.");
                }

                else if (tipodato1.Equals("double") && tipodato2.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) / Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) / Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");
                }
                else if (tipodato1.Equals("double") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number1) / Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] / Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }

                //ints
                else if ((tipodato1.Equals("int") && tipodato2.Equals("string")) || (tipodato2.Equals("int") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar int con string.");
                    resultado.setTipo("Error tipo dato al operar int con string.");
                }

                else if ((tipodato2.Equals("int") && tipodato1.Equals("int")) || (tipodato1.Equals("int") && tipodato2.Equals("int")))
                {
                    resultado.setValor((Convert.ToDouble(number1) / Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato1.Equals("int") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number1) / Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("int") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] / Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }

                //chars 

                else if ((tipodato1.Equals("char") && tipodato2.Equals("string")) || (tipodato2.Equals("char") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar char con string.");
                    resultado.setTipo("Error tipo dato al operar char con string.");
                }

                else if ((tipodato2.Equals("char") && tipodato1.Equals("char")) || (tipodato1.Equals("char") && tipodato2.Equals("char")))
                {
                    resultado.setValor((Convert.ToDouble(Encoding.ASCII.GetBytes(number1)[0]) / Convert.ToDouble(Encoding.ASCII.GetBytes(number2)[0])).ToString());
                    resultado.setTipo("double");
                }

                // string
                else if ((tipodato1.Equals("string") && tipodato2.Equals("string")) || (tipodato2.Equals("string") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar string con string.");
                    resultado.setTipo("Error tipo dato al operar string con string.");
                }
                #endregion
            }
            else if (Operacion.Equals("porcentaje"))
            {
                #region PORCENTAJE

                if (tipodato1.Equals("boolean") && tipodato2.Equals("boolean"))
                {
                    resultado.setValor("Error tipo dato al operar boolean con boolean.");
                    resultado.setTipo("Error tipo dato al operar boolean con boolean.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("double"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 % Convert.ToDouble(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 % Convert.ToDouble(number2)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("double"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 % Convert.ToDouble(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 % Convert.ToDouble(number1)).ToString());
                    }
                    resultado.setTipo("double");
                }

                else if ((tipodato1.Equals("boolean") && tipodato2.Equals("string")) || (tipodato2.Equals("boolean") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar boolean con string.");
                    resultado.setTipo("Error tipo dato al operar boolean con string.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("int"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((1 % Convert.ToDouble(number2)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 % Convert.ToDouble(number2)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("int"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((1 % Convert.ToDouble(number1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((0 % Convert.ToDouble(number1)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if ((tipodato2.Equals("boolean") && tipodato1.Equals("char")) || (tipodato1.Equals("boolean") && tipodato2.Equals("char")))
                {
                    resultado.setValor("Error tipo dato al operar boolean con char.");
                    resultado.setTipo("Error tipo dato al operar boolean con char.");
                }


                //doubles 

                else if ((tipodato2.Equals("double") && tipodato1.Equals("double")) || (tipodato1.Equals("double") && tipodato2.Equals("double")))
                {
                    resultado.setValor((Convert.ToDouble(number1) % Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if ((tipodato1.Equals("double") && tipodato2.Equals("string")) || (tipodato2.Equals("double") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar double con string.");
                    resultado.setTipo("Error tipo dato al operar double con string.");
                }

                else if (tipodato1.Equals("double") && tipodato2.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) % Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("int"))
                {
                    resultado.setValor((Convert.ToDouble(number1) % Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato1.Equals("double") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number1) % Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] % Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }

                //ints
                else if ((tipodato1.Equals("int") && tipodato2.Equals("string")) || (tipodato2.Equals("int") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar int con string.");
                    resultado.setTipo("Error tipo dato al operar int con string.");
                }

                else if ((tipodato2.Equals("int") && tipodato1.Equals("int")) || (tipodato1.Equals("int") && tipodato2.Equals("int")))
                {
                    resultado.setValor((Convert.ToDouble(number1) % Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato1.Equals("int") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Convert.ToDouble(number1) % Encoding.ASCII.GetBytes(number2)[0]).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("int") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Encoding.ASCII.GetBytes(number1)[0] % Convert.ToDouble(number2)).ToString());
                    resultado.setTipo("double");

                }

                //chars 

                else if ((tipodato1.Equals("char") && tipodato2.Equals("string")) || (tipodato2.Equals("char") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar char con string.");
                    resultado.setTipo("Error tipo dato al operar char con string.");
                }

                else if ((tipodato2.Equals("char") && tipodato1.Equals("char")) || (tipodato1.Equals("char") && tipodato2.Equals("char")))
                {
                    resultado.setValor((Convert.ToDouble(Encoding.ASCII.GetBytes(number1)[0]) % Convert.ToDouble(Encoding.ASCII.GetBytes(number2)[0])).ToString());
                    resultado.setTipo("double");
                }

                // string
                else if ((tipodato1.Equals("string") && tipodato2.Equals("string")) || (tipodato2.Equals("string") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar string con string.");
                    resultado.setTipo("Error tipo dato al operar string con string.");
                }
                #endregion
            }
            else if (Operacion.Equals("potencia"))
            {
                #region POTENCIA

                if (tipodato1.Equals("boolean") && tipodato2.Equals("boolean"))
                {
                    resultado.setValor("Error tipo dato al operar boolean con boolean.");
                    resultado.setTipo("Error tipo dato al operar boolean con boolean.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("double"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((Math.Pow(1, Convert.ToDouble(number2))).ToString());
                    }
                    else
                    {
                        resultado.setValor((Math.Pow(0, Convert.ToDouble(number2))).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("double"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((Math.Pow(Convert.ToDouble(number1), 1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((Math.Pow(Convert.ToDouble(number1), 0)).ToString());
                    }
                    resultado.setTipo("double");
                }

                else if ((tipodato1.Equals("boolean") && tipodato2.Equals("string")) || (tipodato2.Equals("boolean") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar boolean con string.");
                    resultado.setTipo("Error tipo dato al operar boolean con string.");
                }

                else if (tipodato1.Equals("boolean") && tipodato2.Equals("int"))
                {
                    if (Convert.ToBoolean(number1))
                    {
                        resultado.setValor((Math.Pow(1, Convert.ToDouble(number2))).ToString());
                    }
                    else
                    {
                        resultado.setValor((Math.Pow(0, Convert.ToDouble(number2))).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("boolean") && tipodato1.Equals("int"))
                {
                    if (Convert.ToBoolean(number2))
                    {
                        resultado.setValor((Math.Pow(Convert.ToDouble(number1), 1)).ToString());
                    }
                    else
                    {
                        resultado.setValor((Math.Pow(Convert.ToDouble(number1), 0)).ToString());
                    }
                    resultado.setTipo("double");
                }
                else if ((tipodato2.Equals("boolean") && tipodato1.Equals("char")) || (tipodato1.Equals("boolean") && tipodato2.Equals("char")))
                {
                    resultado.setValor("Error tipo dato al operar boolean con char.");
                    resultado.setTipo("Error tipo dato al operar boolean con char.");
                }


                //doubles 

                else if ((tipodato2.Equals("double") && tipodato1.Equals("double")) || (tipodato1.Equals("double") && tipodato2.Equals("double")))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(number1), Convert.ToDouble(number2))).ToString());
                    resultado.setTipo("double");

                }
                else if ((tipodato1.Equals("double") && tipodato2.Equals("string")) || (tipodato2.Equals("double") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar double con string.");
                    resultado.setTipo("Error tipo dato al operar double con string.");
                }

                else if (tipodato1.Equals("double") && tipodato2.Equals("int"))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(number1), Convert.ToDouble(number2))).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("int"))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(number1), Convert.ToDouble(number2))).ToString());
                    resultado.setTipo("double");

                }
                else if (tipodato1.Equals("double") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(number1), Convert.ToDouble(Encoding.ASCII.GetBytes(number2)[0]))).ToString());
                    resultado.setTipo("double");
                }
                else if (tipodato2.Equals("double") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(Encoding.ASCII.GetBytes(number1)[0]), Convert.ToDouble(number2))).ToString());
                    resultado.setTipo("double");

                }

                //ints
                else if ((tipodato1.Equals("int") && tipodato2.Equals("string")) || (tipodato2.Equals("int") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar int con string.");
                    resultado.setTipo("Error tipo dato al operar int con string.");
                }

                else if ((tipodato2.Equals("int") && tipodato1.Equals("int")) || (tipodato1.Equals("int") && tipodato2.Equals("int")))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(number1), Convert.ToDouble(number2))).ToString());
                    resultado.setTipo("int");
                }
                else if (tipodato1.Equals("int") && tipodato2.Equals("char"))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(number1), Convert.ToDouble(Encoding.ASCII.GetBytes(number2)[0]))).ToString());
                    resultado.setTipo("int");

                }
                else if (tipodato2.Equals("int") && tipodato1.Equals("char"))
                {
                    resultado.setValor((Math.Pow(Convert.ToDouble(Encoding.ASCII.GetBytes(number1)[0]), Convert.ToDouble(number2))).ToString());
                    resultado.setTipo("int");
                }

                //chars 

                else if ((tipodato1.Equals("char") && tipodato2.Equals("string")) || (tipodato2.Equals("char") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar char con string.");
                    resultado.setTipo("Error tipo dato al operar char con string.");
                }

                else if ((tipodato2.Equals("char") && tipodato1.Equals("char")) || (tipodato1.Equals("char") && tipodato2.Equals("char")))
                {
                    resultado.setValor(Math.Pow(Convert.ToDouble(Encoding.ASCII.GetBytes(number1)[0]), Convert.ToDouble(Encoding.ASCII.GetBytes(number2)[0])).ToString());
                    resultado.setTipo("double");

                }

                // string
                else if ((tipodato1.Equals("string") && tipodato2.Equals("string")) || (tipodato2.Equals("string") && tipodato1.Equals("string")))
                {
                    resultado.setValor("Error tipo dato al operar string con string.");
                    resultado.setTipo("Error tipo dato al operar string con string.");
                }
                #endregion

            }

            return resultado;
        }

        public static Retorno Operaciones(ParseTreeNode Nodo)
        {
            Retorno resultado = new Retorno("", "");

            try
            {
                Retorno number1 = null;
                Retorno number2 = null;

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
                                    number1 = Operaciones(Nodo.ChildNodes[i]);
                                }
                                else if (i == 2)
                                {
                                    number2 = Operaciones(Nodo.ChildNodes[i]);
                                }
                            }
                            resultado = operacionSeguntipos(number1.getTipo(), number2.getTipo(), number1.getValor(), number2.getValor(), "suma");
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
                                    number1 = Operaciones(Nodo.ChildNodes[i]);
                                }
                                else if (i == 2)
                                {
                                    number2 = Operaciones(Nodo.ChildNodes[i]);
                                }
                            }
                            resultado = operacionSeguntipos(number1.getTipo(), number2.getTipo(), number1.getValor(), number2.getValor(), "resta");
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
                                    number1 = Operaciones(Nodo.ChildNodes[i]);
                                }
                                else if (i == 2)
                                {
                                    number2 = Operaciones(Nodo.ChildNodes[i]);
                                }
                            }
                            resultado = operacionSeguntipos(number1.getTipo(), number2.getTipo(), number1.getValor(), number2.getValor(), "multiplicacion");
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
                                    number1 = Operaciones(Nodo.ChildNodes[i]);
                                }
                                else if (i == 2)
                                {
                                    number2 = Operaciones(Nodo.ChildNodes[i]);
                                }
                            }
                            resultado = operacionSeguntipos(number1.getTipo(), number2.getTipo(), number1.getValor(), number2.getValor(), "division");
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
                                    number1 = Operaciones(Nodo.ChildNodes[i]);
                                }
                                else if (i == 2)
                                {
                                    number2 = Operaciones(Nodo.ChildNodes[i]);
                                }
                            }
                            resultado = operacionSeguntipos(number1.getTipo(), number2.getTipo(), number1.getValor(), number2.getValor(), "porcentaje");
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
                                    number1 = Operaciones(Nodo.ChildNodes[i]);
                                }
                                else if (i == 2)
                                {
                                    number2 = Operaciones(Nodo.ChildNodes[i]);
                                }
                            }
                            resultado = operacionSeguntipos(number1.getTipo(), number2.getTipo(), number1.getValor(), number2.getValor(), "potencia");
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
                            number1 = Operaciones(Nodo.ChildNodes[1]);
                            number1.setValor("-" + number1.getValor());
                            resultado = number1;
                            return resultado;
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);
                        }
                        break;
                    case "OPERACIONES7":
                        // OPERACIONES7.Rule = Entero
                        //      | Decimal
                        //    | Id
                        //  | Id + tkCORA + OPERACION + tkCORC       //acceso a una posicion de un arreglo
                        //| Cadena
                        //| LLAMADA                                //llamada a un metodo
                        //| tkGETUSER + tkPARA + tkPARC            //funcion privada  que devuelde el usuario logeado
                        //| tkPARA + OPERACION + tkPARC
                        if (Nodo.ChildNodes[0].Term.Name.ToString().Equals("("))     //vienen parentesis entonces por precedencia se hara primero
                        {
                            resultado = Operaciones(Nodo.ChildNodes[1]);
                        }
                        else if (Nodo.ChildNodes[0].Term.Name.ToString().Equals("LLAMADA"))
                        {
                            String idenasignacion = IdVariable;         //CAPTURO A VARIABLE PORQUE COMO CUANDO ESTA ASIGNACNO Y LLAMA A METODO SE PIERDE EL NOMBRE DE LA VARIABLE QUE ESTA ASIGNANDO
                            EjecucionCodigo(Nodo.ChildNodes[0], AmbitoVariable, Nivel);
                            resultado = Returnmetodo;
                            IdVariable = idenasignacion;
                            Yasedetubo = false;
                        }
                        else if (Nodo.ChildNodes.Count == 4)
                        {
                            String id = Tipos_Y_Id(Nodo.ChildNodes[0]);        //recibira el id
                            Retorno posisicon = Operaciones(Nodo.ChildNodes[2]);

                            for (int i = 0; i < Amb.Count; i++)
                            {
                                Ambitos simb = (Ambitos)Amb[i];
                                if (simb.getNivel() == Nivel || simb.getNivel() == 0)
                                {
                                    List<Simbolos> si = simb.getSimbolos();
                                    for (int j = 0; j < si.Count; j++)
                                    {
                                        Simbolos yeah = (Simbolos)si[j];
                                        if (yeah.GetId().Equals(id) && (yeah.Getnivel() == Nivel || yeah.Getnivel() == 0))
                                        {
                                            if (yeah.GetEsArreglo())
                                            {
                                                String[] Arreglo = yeah.GetValores();
                                                resultado.setValor(Arreglo[Convert.ToInt32(posisicon.getValor())]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            resultado = Operaciones(Nodo.ChildNodes[0]);
                        }
                        break;

                    case "Entero":
                        Retorno resul = new Retorno("int", Nodo.Token.Value.ToString());
                        resultado = resul;
                        break;
                    case "Decimal":
                        Retorno resul1 = new Retorno("double", Nodo.Token.Value.ToString());
                        resultado = resul1;
                        break;
                    case "Cadena":
                        Retorno resul2 = new Retorno("string", Nodo.Token.Value.ToString());
                        resultado = resul2;
                        break;
                    case "Char":
                        Retorno resul5 = new Retorno("char", Nodo.Token.Value.ToString());
                        resultado = resul5;
                        break;

                    case "true":
                    case "false":
                        Retorno resul3 = new Retorno("boolean", Nodo.Token.Value.ToString());
                        resultado = resul3;
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
                                        Retorno resul4 = new Retorno(yeah.GetTipo().ToLower(), yeah.GetValor());
                                        resultado = resul4;

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
                //resultado = "Error Semantico de tipo de datos.";
                return resultado;
            }
        }
    }
}
