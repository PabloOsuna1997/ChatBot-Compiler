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

        public Simbolos(String Id, String Ambito, int Nivel, String Valor, String Tipo)
        {
            this.Id = Id;
            this.Ambito = Ambito;
            this.Nivel = Nivel;
            this.Valor = Valor;
            this.Tipo = Tipo;
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
