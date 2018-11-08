using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2.Analisis
{

    class Retorno
    {
        String Tipo;
        String Valor;
        public Retorno(String Tipo, String Valor)
        {
            this.Tipo = Tipo;
            this.Valor = Valor;
        }

        public String getTipo()
        {
            return Tipo;
        }
        public void setTipo(String Tipo)
        {
            this.Tipo = Tipo;
        }
        public String getValor()
        {
            return Valor;
        }
        public void setValor(String Valor)
        {
            this.Valor = Valor;
        }
    }
}
