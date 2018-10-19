using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Irony.Parsing;
using Irony.Ast;

namespace Proyecto2.Analisis
{
    class Gramatica : Grammar
    {
        public Gramatica() : base(caseSensitive: true)
        {
            #region Expresiones Regulares 
            //numeros 
            RegexBasedTerminal Entero = new RegexBasedTerminal("Entero", "[0-9]+");
            RegexBasedTerminal Decimal = new RegexBasedTerminal("Decimal", "[0-9]+[.][0-9]+");
            RegexBasedTerminal IDMetodo = new RegexBasedTerminal("IDMetodo", "[a-zA-Z]+[_]?[0-9]*");

            //id
            IdentifierTerminal Id = new IdentifierTerminal("Id");

            //cadena 
            StringLiteral Cadena = new StringLiteral("Cadena", "\"");

            //comentarios
            CommentTerminal COMENTARIOLINEA = new CommentTerminal("LINE_COMMENT", "//", "\n", "\r\n");
            CommentTerminal COMENTARIOBLOQUE = new CommentTerminal("BLOCK_COMMENT", "/*", "*/");
            #endregion


            #region Terminales
            //tipos de variables.
            var tkVOID = ToTerm("Void");
            var tkINT = ToTerm("Int");
            var tkFLOAT = ToTerm("Float");
            var tkBOOL = ToTerm("Boolean");
            var tkCHAR = ToTerm("Char");
            var tkSTRING = ToTerm("String");
            var tkDOUBLE = ToTerm("Double");

            var tkMAIN = ToTerm("main");
            var tkRETURN = ToTerm("Return");
            var tkTRUE = ToTerm("true");
            var tkFALSE = ToTerm("false");
            var tkIMPORT = ToTerm("Import");
            var tkFOR = ToTerm("For");
            var tkIF = ToTerm("If");
            var tkELSE = ToTerm("Else");
            var tkDO = ToTerm("Do");
            var tkWHILE = ToTerm("While");
            var tkSWITCH = ToTerm("Switch");
            var tkCASE = ToTerm("Case");
            var tkDEFAULT = ToTerm("Default");
            var tkBREAK = ToTerm("Break");
            var tkCOMPARE = ToTerm("CompareTo");
            var tkGETUSER = ToTerm("GetUser");
            var tkPRINT = ToTerm("Print");
            var ESPACIO = ToTerm(" ");

            //Simbolos
            var tkMAS = ToTerm("+");
            var tkMENOS = ToTerm("-");
            var tkPOR = ToTerm("*");
            var tkDIV = ToTerm("/");
            var tkPORCENT = ToTerm("%");
            var tkPOTENCIA = ToTerm("^");
            var tkPARA = ToTerm("(");
            var tkPARC = ToTerm(")");
            var tkLLAVA = ToTerm("{");
            var tkLLAVC = ToTerm("}");
            var tkCORA = ToTerm("[");
            var tkCORC = ToTerm("]");
            var tkMENOR = ToTerm("<");
            var tkMAYOR = ToTerm(">");
            var tkMENORIGUAL = ToTerm("<=");
            var tkMAYORIGUAL = ToTerm(">=");
            var tkIGUAL = ToTerm("=");
            var tkDISTINTO = ToTerm("!");
            var tkOR = ToTerm("||");
            var tkXOR = ToTerm("!&");
            var tkAND = ToTerm("&&");
            var tkPUNTO = ToTerm(".");
            var tkPUNTOYCOMA = ToTerm(";");
            var tkCOMA = ToTerm(",");
            var tkDOSPUNTOS = ToTerm(":");
            var tkAPOSTRO = ToTerm("\'");

            #endregion

            #region No Terminales
            NonTerminal S = new NonTerminal("S"),
                INICIO = new NonTerminal("INICIO"),
                IMPORTS = new NonTerminal("IMPORTS"),
                IMPORTACION = new NonTerminal("IMPORTACION"),
                SENTENCIAS = new NonTerminal("SENTENCIAS"),
                LISTAIDS = new NonTerminal("LISTAIDS"),
                TIPO = new NonTerminal("TIPO"),
                DECLARACIONVARIABLES = new NonTerminal("DECLARACIONVARIABLES"),
                MASASIGNACION = new NonTerminal("MASASIGNACION"),
                ASIGNACION = new NonTerminal("ASIGNACION"),
                OPERACION = new NonTerminal("OPERACION"),
                OPERACIONES1 = new NonTerminal("OPERACIONES1"),
                OPERACIONES2 = new NonTerminal("OPERACIONES2"),
                OPERACIONES3 = new NonTerminal("OPERACIONES3"),
                OPERACIONES4 = new NonTerminal("OPERACIOENS4"),
                OPERACIONES5 = new NonTerminal("OPERACIONES5"),
                OPERACIONES6 = new NonTerminal("OPERACIONES6"),
                OPERACIONES7 = new NonTerminal("OPERACIONES7"),
                DECLARACIONMETODOS = new NonTerminal("DECLARACIONMETODOS"),
                LISTAPARAMETROS = new NonTerminal("LISTAPARAMETROS"),
                LISTASENTENCIAS = new NonTerminal("LISTASENTENCIAS"),
                PARAMETRO = new NonTerminal("PARAMETRO"),
                IF = new NonTerminal("IF"),
                CONDICIONES = new NonTerminal("CONDICIONES"),
                BLOQUEELSE = new NonTerminal("BLOQUEELSE"),
                WHILE = new NonTerminal("WHILE"),
                DO = new NonTerminal("DO"),
                CONDICIONES1 = new NonTerminal("CONDICIONES1"),
                CONDICIONES2 = new NonTerminal("CONDICIONES2"),
                CONDICIONES3 = new NonTerminal("CONDICIONES3"),
                CONDICIONES4 = new NonTerminal("CONDICIONES4"),
                CONDICIONES5 = new NonTerminal("CONDICIONES5"),
                CONDICIONES6 = new NonTerminal("CONDICIONES6"),
                CONDICIONES7 = new NonTerminal("CONDICIONES7"),
                CONDICIONES8 = new NonTerminal("CONDICIONES8"),
                CONDICIONES9 = new NonTerminal("CONDICIONES9"),
                CONDICIONES10 = new NonTerminal("CONDICIONES10"),
                SWITCH = new NonTerminal("SWITCH"),
                LISTACASOS = new NonTerminal("LISTACASOS"),
                CASOS = new NonTerminal("CASOS"),
                DEFAULT = new NonTerminal("DEFAULT"),
                FOR = new NonTerminal("FOR"),
                INCREODECRE = new NonTerminal("INCREODECRE"),
                PRINT = new NonTerminal("PRINT"),
                LISTAEXPRESIONES = new NonTerminal("LISTAEXPRESIONES"),
                LLAMADA = new NonTerminal("LLAMADA"),
                LISTAPARAMETROSLLAMADA = new NonTerminal("LISTAPARAMETROSLLAMADA"),
                PAR = new NonTerminal("PAR"),
                RETURN = new NonTerminal("RETURN"),
                ASIGNACIONVAR = new NonTerminal("ASIGNACIONVAR"),
                VALOR = new NonTerminal("VALOR"),
                ACCIONES = new NonTerminal("ACCIONES"),
                DECLARACIONARREGLOS = new NonTerminal("DECLARACIONARREGLOS"),
                AC = new NonTerminal("AC"),
                INICIALIZACIONARREGLO = new NonTerminal("INICIALIZACIONARREGLO"),
                LISTADATOS = new NonTerminal("LISTADATOS"),
                ASIGNACIONARREGLO = new NonTerminal("ASIGNACIONARREGLO"),
                MASASIG = new NonTerminal("MASASIG");
            #endregion

            #region Gramatica

            S.Rule = IMPORTS + INICIO;
            ;

            IMPORTS.Rule = IMPORTS + IMPORTACION
                         | IMPORTACION
            ;

            IMPORTACION.Rule = tkIMPORT + Cadena + tkPUNTOYCOMA
                               | Empty
            ;

            INICIO.Rule = INICIO + ACCIONES
                         | ACCIONES;

            //UNICAS ACCIONES QUE SE REALIZARAN DE FORMA GLOBAL.
            ACCIONES.Rule = //declaraciones
                             DECLARACIONVARIABLES
                            | DECLARACIONMETODOS
                            | DECLARACIONARREGLOS
                            //asignaciones
                            | ASIGNACIONARREGLO
                            | ASIGNACIONVAR;

            #region DECLARACION DE ARREGLO

            DECLARACIONARREGLOS.Rule = Id + tkDOSPUNTOS + TIPO + tkCORA + OPERACION + tkCORC + INICIALIZACIONARREGLO + tkPUNTOYCOMA;

            INICIALIZACIONARREGLO.Rule = tkIGUAL + tkLLAVA + LISTADATOS + tkLLAVC
                                      | Empty;

            LISTADATOS.Rule = LISTADATOS + tkCOMA + CONDICIONES
                                | CONDICIONES;
            #endregion

            #region ASIGNACION DE ARREGLOS

            ASIGNACIONARREGLO.Rule = Id + tkIGUAL + INICIALIZACIONARREGLO + tkPUNTOYCOMA
                                     | Id + tkCORA + OPERACION + tkCORC + tkIGUAL + CONDICIONES + tkPUNTOYCOMA;

            #endregion

            #region DECLARACION DE VARIABLES MULTIPLES

            DECLARACIONVARIABLES.Rule = Id + MASASIG;

            MASASIG.Rule = tkCOMA + LISTAIDS + tkDOSPUNTOS + TIPO + ASIGNACION + tkPUNTOYCOMA
                            | tkDOSPUNTOS + TIPO + ASIGNACION + tkPUNTOYCOMA;

            LISTAIDS.Rule = LISTAIDS + tkCOMA + Id
                            | Id;

            TIPO.Rule = tkINT
                        | tkDOUBLE
                        | tkSTRING
                        | tkCHAR
                        | tkBOOL;

            ASIGNACION.Rule = tkIGUAL + CONDICIONES  //<-- (OPERACIONES ARITMETICAS)
                              | Empty;

            #endregion

            #region ASIGNACION DE VARIABLE

            ASIGNACIONVAR.Rule = Id + tkIGUAL + CONDICIONES + tkPUNTOYCOMA;
            #endregion

            #region DECLARACION METODOS 

            DECLARACIONMETODOS.Rule = Id + tkDOSPUNTOS + TIPO + tkPARA + LISTAPARAMETROS + tkPARC + tkLLAVA + LISTASENTENCIAS + tkLLAVC;

            LISTAPARAMETROS.Rule = LISTAPARAMETROS + tkCOMA + PARAMETRO
                                   | PARAMETRO;

            PARAMETRO.Rule = Id + tkDOSPUNTOS + TIPO
                             | Empty;

            #endregion

            #region LISTASENTENCIAS

            LISTASENTENCIAS.Rule = LISTASENTENCIAS + SENTENCIAS     //a esta produccion caeran todas las sentencias de metodos, if´s, etc.
                                  | SENTENCIAS;

            SENTENCIAS.Rule = IF
                             | SWITCH
                             | FOR
                             | DO
                             | WHILE
                             | PRINT
                             | LLAMADA + tkPUNTOYCOMA
                             | tkRETURN + OPERACION + tkPUNTOYCOMA        //return
                             | tkBREAK + tkPUNTOYCOMA                     //break
                             | ACCIONES
                             | Empty;//   |PRINT;
            #endregion

            #region IF-ELSE
            IF.Rule = tkIF + tkPARA + CONDICIONES + tkPARC + tkLLAVA + LISTASENTENCIAS + tkLLAVC + BLOQUEELSE;

            BLOQUEELSE.Rule = tkELSE + tkLLAVA + LISTASENTENCIAS + tkLLAVC
                              | Empty;
            #endregion

            #region SWITCH

            SWITCH.Rule = tkSWITCH + tkPARA + OPERACION + tkPARC + tkLLAVA + LISTACASOS + DEFAULT + tkLLAVC;

            LISTACASOS.Rule = LISTACASOS + CASOS
                              | CASOS;

            CASOS.Rule = tkCASE + OPERACION + tkDOSPUNTOS + LISTASENTENCIAS + tkBREAK + tkPUNTOYCOMA;

            DEFAULT.Rule = tkDEFAULT + OPERACION + tkDOSPUNTOS + LISTASENTENCIAS + tkBREAK + tkPUNTOYCOMA
                           | Empty;

            #endregion

            #region FOR
            FOR.Rule = tkFOR + tkPARA + DECLARACIONVARIABLES + CONDICIONES + tkPUNTOYCOMA + Id + INCREODECRE + tkPARC + tkLLAVA + LISTASENTENCIAS + tkLLAVC;

            INCREODECRE.Rule = tkMAS + tkMAS
                               | tkMENOS + tkMENOS;
            #endregion

            #region DO-WHILE

            DO.Rule = tkDO + tkLLAVA + LISTASENTENCIAS + tkLLAVC + tkWHILE + tkPARA + CONDICIONES + tkPARC + tkPUNTOYCOMA;
            #endregion

            #region WHILE

            WHILE.Rule = tkWHILE + tkPARA + CONDICIONES + tkPARC + tkLLAVA + LISTASENTENCIAS + tkLLAVC;

            #endregion

            #region PRINT

            PRINT.Rule = tkPRINT + tkPARA + LISTAEXPRESIONES + tkPARC + tkPUNTOYCOMA;

            LISTAEXPRESIONES.Rule = LISTAEXPRESIONES + tkMAS + OPERACION
                                    | OPERACION;

            #endregion

            #region LLAMADA METODO
            LLAMADA.Rule = Id + tkPARA + PAR + tkPARC;

            PAR.Rule = LISTAPARAMETROSLLAMADA
                       | Empty;

            LISTAPARAMETROSLLAMADA.Rule = LISTAPARAMETROSLLAMADA + tkCOMA + CONDICIONES
                                          | CONDICIONES;


            #endregion

            #region OPERACIONES ARITMETICAS

            OPERACION.Rule = OPERACION + tkMAS + OPERACIONES1
                              | OPERACIONES1;

            OPERACIONES1.Rule = OPERACIONES1 + tkMENOS + OPERACIONES2
                               | OPERACIONES2;
            ;

            OPERACIONES2.Rule = OPERACIONES2 + tkPOR + OPERACIONES3
                               | OPERACIONES3;
            ;

            OPERACIONES3.Rule = OPERACIONES3 + tkDIV + OPERACIONES4
                                | OPERACIONES4;
            ;

            OPERACIONES4.Rule = OPERACIONES4 + tkPORCENT + OPERACIONES5
                                | OPERACIONES5;
            ;

            OPERACIONES5.Rule = OPERACIONES5 + tkPOTENCIA + OPERACIONES6
                              | OPERACIONES6;

            OPERACIONES6.Rule = tkMENOS + OPERACIONES7
                                | OPERACIONES7;

            OPERACIONES7.Rule = Entero
                               | Decimal
                               | Id
                               | Id + tkCORA + OPERACION + tkCORC       //acceso a una posicion de un arreglo
                               | Cadena
                               | LLAMADA                                //llamada a un metodo
                               | tkGETUSER + tkPARA + tkPARC            //funcion privada  que devuelde el usuario logeado
                               | tkPARA + OPERACION + tkPARC
            ;
            #endregion

            #region OPERACIONES LOGICAS Y RELACIONALES (CONDICIONES)

            CONDICIONES.Rule = CONDICIONES + tkIGUAL + tkIGUAL + CONDICIONES1
                               | CONDICIONES1
            ;

            CONDICIONES1.Rule = CONDICIONES1 + tkDISTINTO + tkIGUAL + CONDICIONES2
                           | CONDICIONES2
            ;

            CONDICIONES2.Rule = CONDICIONES2 + tkMAYORIGUAL + CONDICIONES3
                               | CONDICIONES3
           ;

            CONDICIONES3.Rule = CONDICIONES3 + tkMENORIGUAL + CONDICIONES4
                                | CONDICIONES4
            ;

            CONDICIONES4.Rule = CONDICIONES4 + tkMAYOR + CONDICIONES5
                                | CONDICIONES5
            ;

            CONDICIONES5.Rule = CONDICIONES5 + tkMENOR + CONDICIONES6
                                | CONDICIONES6
            ;

            CONDICIONES6.Rule = CONDICIONES6 + tkOR + CONDICIONES7
                                | CONDICIONES7
            ;

            CONDICIONES7.Rule = CONDICIONES7 + tkXOR + CONDICIONES8
                                | CONDICIONES8
            ;

            CONDICIONES8.Rule = CONDICIONES8 + tkAND + CONDICIONES9
                                | CONDICIONES9
            ;

            CONDICIONES9.Rule = CONDICIONES9 + tkDISTINTO + CONDICIONES10
                              | CONDICIONES10
          ;

            CONDICIONES10.Rule = OPERACION
                                | tkTRUE
                                | tkFALSE
                                | Id + tkPUNTO + tkCOMPARE + tkPARA + Cadena + tkPARC           //compare porque siempre devolvera un tru o un false
                                | tkPARA + CONDICIONES + tkPARC
            ;
            #endregion

            #region Comentarios
            //agregamos los comentarios y si vienen que no haga nada.
            NonGrammarTerminals.Add(COMENTARIOLINEA);
            NonGrammarTerminals.Add(COMENTARIOBLOQUE);
            #endregion


            #endregion

            #region Estado de Inicio
            this.Root = S;
            #endregion
        }
    }
}
