using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace caPatchCEPCorreios.Classe
{
    public class Layout
    {
        #region Properties

        public string Municipio { get; set; }
        public string Bairro { get; set; }
        public string TipoLogradouro { get; set; }

        public string Logradouro { get; set; }

        public string Cep { get; set; }

        public string Uf { get; set; }

        public string Preposicao { get; set; }

        #endregion
    }
}
