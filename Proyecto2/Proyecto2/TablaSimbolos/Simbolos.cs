using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2.TablaSimbolos
{
    class Simbolos
    {
        String Id;
        String Ambito;
        int Nivel;
        String Valor;
        String Tipo;
        String[] Valores;
        Boolean EsArreglo;

        public Simbolos(String Id, String Ambito, int Nivel, String Valor, String Tipo,Boolean EsArreglo)
        {
            this.Id = Id;
            this.Ambito = Ambito;
            this.Nivel = Nivel;
            this.Valor = Valor;
            this.Tipo = Tipo;
            this.EsArreglo = EsArreglo;
        }

        public Simbolos(String Id, String Ambito, int Nivel, String[] Valores, String Tipo,Boolean EsArreglo)
        {
            this.Id = Id;
            this.Ambito = Ambito;
            this.Nivel = Nivel;
            this.Tipo = Tipo;
            this.Valores = Valores;
            this.EsArreglo = EsArreglo;
        }

        public Boolean GetEsArreglo()
        {
            return EsArreglo;
        }
        public void SetEsArreglo (Boolean EsArreglo)
        {
            this.EsArreglo = EsArreglo;
        }

        public String[] GetValores()
        {
            return Valores;
        }
        public void SetValores(String[] Valores)
        {
            this.Valores = Valores;
        }

        public String GetId()
        {
            return Id;
        }
        public void SetId(String Id)
        {
            this.Id = Id;
        }
        public String GetAmbito()
        {
            return Ambito;
        }
        public void SetAmbito(String Ambito)
        {
            this.Ambito = Ambito;
        }
        public int Getnivel()
        {
            return Nivel;
        }
        public void SetNivel(int Nivel)
        {
            this.Nivel = Nivel;
        }
        public String GetValor()
        {
            return Valor;
        }
        public void SetValor(String Valor)
        {
            this.Valor = Valor;
        }
        public String GetTipo()
        {
            return Tipo;
        }
        public void SetTipo(String Tipo)
        {
            this.Tipo = Tipo;
        }

    }
}
