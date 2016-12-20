using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caPatchCEPCorreios.Classe
{
    public class LayoutAtualizacaoLocalidade
    {
        public string Uf { get; set; }
        public string Municipio { get; set; }
        public string Cep { get; set; }
        public string CepAnterior { get; set; }
        public string Tipo { get; set; }
        public string Operacao { get; set; }
    }
}
