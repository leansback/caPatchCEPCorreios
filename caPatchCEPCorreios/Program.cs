using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using caPatchCEPCorreios.Classe;
using System.Data.EntityClient;
using System.Data.Linq;
using log4net;
using System.Reflection;

namespace caPatchCEPCorreios
{
    class Program
    {
        #region Variáveis de controle de código
        
        static int proxCodigoLogradouro = 0;

        static int proxCodigoLocalidade = 0;

        static int proxCodigoBairro = 0;

        #endregion

        static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            Console.WriteLine("Atualizando Localidades...");

            log.Debug("...Atualizando Localidades...");

            AtualizaLocalidades();

            Console.WriteLine("Localidades atualizadas com sucesso!");

            Console.WriteLine("Atualizando Bairros...");

            AtualizaBairro();

            Console.WriteLine("Bairros atualizados com sucesso!");

            Console.WriteLine("Atualizando Logradouros...");

            AtualizaLogradouro();

            Console.WriteLine("Logradouros atualizados com sucesso!");
            
            AtualizaGrandeUsuario();

            Console.WriteLine("Grande usuário atualizados com sucesso!");
            
            Console.ReadKey();

        }

        //#region Definindo Contexto

        //    CorporeRM_FabaoEntities context = new CorporeRM_FabaoEntities();

        //    context.Configuration.AutoDetectChangesEnabled = true;

        //    context.Configuration.LazyLoadingEnabled = true;

        //    #endregion

        static private CorporeRM_FabaoEntities DefinirContexto()
        {
            #region Definindo Contexto

            CorporeRM_FabaoEntities context = new CorporeRM_FabaoEntities();

            context.Configuration.AutoDetectChangesEnabled = true;

            context.Configuration.LazyLoadingEnabled = true;

            #endregion

            return context;
        }

        #region Métodos de Atualização de base

        private static void AtualizaLocalidades()
        {
            try
            {
                #region Busca do diretório dos arquivos

                string dir = Environment.CurrentDirectory + @"\ArquivoLocalidade";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                DirectoryInfo infoDir = new DirectoryInfo(dir);

                #endregion

                if (infoDir != null)
                {
                    FileInfo[] fileNames = infoDir.GetFiles("*LOCALIDADES.TXT");

                    IOrderedEnumerable<FileInfo> arquivosOrdenados = fileNames.OrderBy(f => f.FullName);

                    foreach (FileInfo fileName in arquivosOrdenados)
                    {
                        CorporeRM_FabaoEntities context = DefinirContexto();

                        if (!fileName.FullName.Contains("DNE_DLT_LOCALIDADES"))
                        {
                            #region Arquivo de Inclusão

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList(); //File.ReadAllLines(fileName.FullName, Encoding.UTF8).ToList();

                            lstLines.RemoveAt(0);

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    #region Monta as informações provenientes do arquivo

                                    LayoutLocalidade objLayoutLocalidade = MontarLayoutLocalidade(line);

                                    #endregion

                                    #region Busca o município na GLocalidade

                                    GLOCALIDADE localidade = new GLOCALIDADE();

                                    localidade = BuscarLocalidade(objLayoutLocalidade.Uf, objLayoutLocalidade.Municipio, context,objLayoutLocalidade.Cep);

                                    #endregion

                                    #region Add-Update Localidades

                                    if (localidade == null)
                                    {
                                        localidade = new GLOCALIDADE();

                                        localidade.UF = objLayoutLocalidade.Uf;

                                        localidade.NOME = objLayoutLocalidade.Municipio;

                                        localidade.CEP = objLayoutLocalidade.Cep;

                                        localidade.CODLOCALIDADE = BuscaCodigoLocalidade(context);

                                        context.GLOCALIDADE.Add(localidade);

                                        log.Debug("Nova localidade encontrada: ");

                                        log.Debug(localidade.CODLOCALIDADE);
                                    }
                                    else
                                    {
                                        if ((!String.IsNullOrEmpty(objLayoutLocalidade.Cep)) && (!String.IsNullOrEmpty(localidade.CEP)) && (!localidade.CEP.Equals(objLayoutLocalidade.Cep)))
                                        {
                                            log.Debug("Localidade Atualizada: ");

                                            log.Debug(localidade.CODLOCALIDADE);

                                            log.Debug("Cep Antigo: ");

                                            log.Debug(localidade.CEP);

                                            log.Debug("Cep Novo: ");

                                            log.Debug(objLayoutLocalidade.Cep);

                                            localidade.CEP = objLayoutLocalidade.Cep;

                                        }
                                    }

                                    #endregion
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            #region Arquivo de Alteração

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList();

                            lstLines.RemoveAt(0);

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    #region Monta as informações provenientes do arquivo

                                    LayoutAtualizacaoLocalidade objLayoutAtualizacaoLocalidade = MontarLayoutAlteracaoLocalidade(line);

                                    #endregion

                                    #region Add-Update-Delete Localidades

                                    GLOCALIDADE localidade = new GLOCALIDADE();

                                    Console.WriteLine("Localidade: " + objLayoutAtualizacaoLocalidade.Municipio);

                                    switch (objLayoutAtualizacaoLocalidade.Operacao)
                                    {
                                        case "D":

                                            //#region Busca o município na GLocalidade

                                            //localidade = BuscarLocalidade(objLayoutAtualizacaoLocalidade.Uf, objLayoutAtualizacaoLocalidade.Municipio, context, objLayoutAtualizacaoLocalidade.Cep);

                                            //#endregion

                                            //if (localidade != null)
                                            //    context.GLOCALIDADE.Remove(localidade);

                                            break;
                                        case "I":

                                            localidade = BuscarLocalidade(objLayoutAtualizacaoLocalidade.Uf, objLayoutAtualizacaoLocalidade.Municipio, context, objLayoutAtualizacaoLocalidade.Cep);

                                            if (localidade == null)
                                            {
                                                localidade = new GLOCALIDADE();

                                                localidade.UF = objLayoutAtualizacaoLocalidade.Uf;

                                                localidade.NOME = objLayoutAtualizacaoLocalidade.Municipio;

                                                localidade.CEP = objLayoutAtualizacaoLocalidade.Cep;

                                                localidade.CODLOCALIDADE = BuscaCodigoLocalidade(context);

                                                context.GLOCALIDADE.Add(localidade);

                                                log.Debug("Nova localidade encontrada: ");

                                                log.Debug(localidade.CODLOCALIDADE);
                                            }

                                            break;
                                        case "U":

                                            localidade = BuscarLocalidade(objLayoutAtualizacaoLocalidade.Uf, objLayoutAtualizacaoLocalidade.Municipio, context, objLayoutAtualizacaoLocalidade.CepAnterior);

                                            if (localidade != null)
                                            {
                                                if ((!String.IsNullOrEmpty(objLayoutAtualizacaoLocalidade.Cep)) && (!localidade.CEP.Equals(objLayoutAtualizacaoLocalidade.Cep)))
                                                {
                                                    log.Debug("Localidade Atualizada: ");

                                                    log.Debug(localidade.CODLOCALIDADE);

                                                    log.Debug("Cep Antigo: ");

                                                    log.Debug(localidade.CEP);

                                                    log.Debug("Cep Novo: ");

                                                    log.Debug(objLayoutAtualizacaoLocalidade.Cep);

                                                    localidade.CEP = objLayoutAtualizacaoLocalidade.Cep;

                                                }

                                            }

                                            break;
                                        default:
                                            break;
                                    }

                                    #endregion
                                }
                            }

                            #endregion
                        }

                        context.SaveChanges();

                        context.Dispose();

                        #region Remover Arquivo para pasta referente a arquivos já processados

                        //MudarPastaLocalidade(fileName);

                        #endregion

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ocorrido durante o processo de Add-Upp de Localidade");

                log.Error("Erro ocorrido durante o processo de Add-Upp de Localidade: ", ex);

                log.Error(ex.InnerException);
            }

        }

        private static void AtualizaBairro()
        {
            try
            {
                #region Busca do diretório dos arquivos
            
                string dir = Environment.CurrentDirectory + @"\ArquivoBairro";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                DirectoryInfo infoDir = new DirectoryInfo(dir);

                #endregion

                if (infoDir != null)
                {
                    FileInfo[] fileNames = infoDir.GetFiles("*BAIRROS.txt");

                    IOrderedEnumerable<FileInfo> arquivosOrdenados = fileNames.OrderByDescending(f => f.FullName);

                    foreach (FileInfo fileName in arquivosOrdenados)
                    {
                        CorporeRM_FabaoEntities context = DefinirContexto();
                        
                        if (fileName.FullName.Contains("DNE_GU_BAIRROS"))
                        {
                            #region Arquivo de Inclusão

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList();

                            lstLines.RemoveAt(0);

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    #region Monta as informações provenientes do arquivo

                                    LayoutBairro objLayoutBairro = MontarLayoutBairro(line);

                                    #endregion

                                    #region Add-Update Bairros

                                    GBAIRRO bairro = BuscarBairro(objLayoutBairro.Uf, objLayoutBairro.Bairro, context, objLayoutBairro.Municipio);

                                    if (bairro == null)
                                    {
                                        bairro = new GBAIRRO();

                                        bairro.UF = objLayoutBairro.Uf;

                                        bairro.CODLOCALIDADE = BuscarLocalidade(objLayoutBairro.Uf, objLayoutBairro.Municipio, context).CODLOCALIDADE;

                                        bairro.NOME = objLayoutBairro.Bairro;

                                        bairro.ABREVIATURA = objLayoutBairro.NomeAbreviado;

                                        bairro.CODBAIRRO = BuscarCodigoBairro(context);

                                        context.GBAIRRO.Add(bairro);

                                        log.Debug("Novo bairro encontrado: ");

                                        log.Debug(bairro.CODBAIRRO);

                                    }
                                    
                                    #endregion
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            #region Arquivo de Atualização

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList();

                            lstLines.RemoveAt(0);

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    #region Monta as informações provenientes do arquivo

                                    LayoutAtualizacaoBairro objLayoutAtualizacaoBairro = MontarLayoutAlteracaoBairro(line);

                                    #endregion

                                    #region Add-Update-Delete Bairros

                                    Console.WriteLine("Bairro: " + objLayoutAtualizacaoBairro.Bairro);

                                    GBAIRRO bairro = BuscarBairro(objLayoutAtualizacaoBairro.Uf, objLayoutAtualizacaoBairro.Bairro, context, objLayoutAtualizacaoBairro.Municipio);

                                    switch (objLayoutAtualizacaoBairro.Operacao)
                                    {
                                        case "D":

                                            //if (bairro != null)
                                            //{
                                            //    context.GBAIRRO.Remove(bairro);
                                            //}

                                            break;
                                        case "I":

                                            if (bairro == null)
                                            {
                                                bairro = new GBAIRRO();

                                                bairro.UF = objLayoutAtualizacaoBairro.Uf;

                                                bairro.CODLOCALIDADE = BuscarLocalidade(objLayoutAtualizacaoBairro.Uf, objLayoutAtualizacaoBairro.Municipio, context).CODLOCALIDADE;

                                                bairro.NOME = objLayoutAtualizacaoBairro.Bairro;

                                                bairro.ABREVIATURA = objLayoutAtualizacaoBairro.NomeAbreviado;

                                                bairro.CODBAIRRO = BuscarCodigoBairro(context);

                                                context.GBAIRRO.Add(bairro);

                                                log.Debug("Novo bairro encontrado: ");

                                                log.Debug(bairro.CODBAIRRO);
                                            }

                                            break;
                                        case "U":

                                            if (bairro != null)
                                            {
                                                if ((!String.IsNullOrEmpty(objLayoutAtualizacaoBairro.NomeAbreviado)) && (!bairro.ABREVIATURA.Equals(objLayoutAtualizacaoBairro.NomeAbreviado)))
                                                {
                                                    log.Debug("Bairro Atualizado: ");

                                                    log.Debug(bairro.CODBAIRRO);

                                                    log.Debug("Abreviatura antiga:");

                                                    log.Debug(bairro.ABREVIATURA);

                                                    log.Debug("Abreviatura nova:");

                                                    log.Debug(objLayoutAtualizacaoBairro.NomeAbreviado);

                                                    bairro.ABREVIATURA = objLayoutAtualizacaoBairro.NomeAbreviado;
                                                }
                                            }
                                            else
                                            {
                                                
                                                bairro = new GBAIRRO();

                                                bairro.UF = objLayoutAtualizacaoBairro.Uf;
                                                log.Debug("Bairro Incluido: " + objLayoutAtualizacaoBairro.Bairro);

                                                bairro.CODLOCALIDADE = BuscarLocalidade(objLayoutAtualizacaoBairro.Uf, objLayoutAtualizacaoBairro.Municipio, context).CODLOCALIDADE;

                                                bairro.NOME = objLayoutAtualizacaoBairro.Bairro;

                                                bairro.ABREVIATURA = objLayoutAtualizacaoBairro.NomeAbreviado;

                                                bairro.CODBAIRRO = BuscarCodigoBairro(context);

                                                context.GBAIRRO.Add(bairro);

                                                log.Debug("Novo bairro encontrado: ");

                                                log.Debug(bairro.CODBAIRRO);


                                            }

                                            break;
                                    }

                                    #endregion
                                }
                            }

                            #endregion
                        }

                        context.SaveChanges();

                        context.Dispose();

                        #region Remover Arquivo para pasta referente a arquivos já processados

                        //MudarPastaBairro(fileName.FullName);

                        #endregion
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ocorrido durante o processo de Add-Upp de Bairro");

                log.Error("Erro ocorrido durante o processo de Add-Upp de Bairro: ", ex);

                log.Error(ex.InnerException);
            }
        }

        private static void AtualizaLogradouro()
        {
            try
            {
                #region Busca do diretório dos arquivos
            
                string dir = Environment.CurrentDirectory + @"\ArquivoLogradouro";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                DirectoryInfo infoDir = new DirectoryInfo(dir);

                #endregion

                if (infoDir != null)
                {
                    FileInfo[] fileNames = infoDir.GetFiles("*_LOGRADOUROS.txt");

                    IOrderedEnumerable<FileInfo> arquivosOrdenados = fileNames.OrderBy(f => f.FullName);

                    foreach (FileInfo fileName in arquivosOrdenados)
                    {
                        if (fileName.FullName.Contains("DNE_DLT_LOGRADOUROS"))
                        {
                            CorporeRM_FabaoEntities context = DefinirContexto();

                            #region Arquivo de Alteração

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList();

                            lstLines.RemoveAt(0);

                            List<GLOGRADOURO> lstLogradourosNovos = new List<GLOGRADOURO>();

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    #region Monta as informações provenientes do arquivo

                                    LayoutAtualizacao objLayoutAtualizacao = MontarLayoutAtualizacao(line);

                                    #endregion
                                    
                                    #region Busca o município na GLocalidade

                                    GLOCALIDADE localidade = BuscarLocalidade(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Municipio, context);

                                    #endregion

                                    #region Busca o bairro na Gbairro

                                    GBAIRRO bairro = BuscarBairro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.BairroInicio, context, localidade.NOME);

                                    #endregion

                                    #region Add-Update-Delete Logradouros

                                    GLOGRADOURO logradouro = new GLOGRADOURO();
                                    
                                    Console.WriteLine("Logradouro: " + objLayoutAtualizacao.Logradouro);

                                    switch (objLayoutAtualizacao.Operacao)
                                    {
                                        case "D":

                                            //logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Cep, context, localidade, bairro);

                                            //if (logradouro != null)
                                            //    context.GLOGRADOURO.Remove(logradouro);
                                            break;

                                        case "I":


                                            if (localidade != null && bairro != null)
                                            {

                                                logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Cep, context, localidade, bairro);

                                                if (logradouro == null)
                                                {
                                                    logradouro = new GLOGRADOURO();

                                                    logradouro.CEP = objLayoutAtualizacao.Cep;

                                                    logradouro.CODBAIRROINI = bairro.CODBAIRRO;

                                                    logradouro.CODBAIRROFIM = bairro.CODBAIRRO; //BuscarBairro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.BairroFim, context, objLayoutAtualizacao.Municipio).CODBAIRRO;

                                                    logradouro.CODLOCALIDADE = localidade.CODLOCALIDADE;

                                                    logradouro.CODLOGRADOURO = BuscaCodigoLogradouro(context);

                                                    logradouro.NOME = objLayoutAtualizacao.Logradouro;

                                                    logradouro.TIPO = objLayoutAtualizacao.TipoLogradouro;

                                                    logradouro.UF = objLayoutAtualizacao.Uf;

                                                    logradouro.UTILIZATIPO = "S";

                                                    context.GLOGRADOURO.Add(logradouro);

                                                    log.Debug("Codigo Logradouro: ");

                                                    log.Debug(logradouro.CODLOGRADOURO);

                                                }
                                            }
                                            else
                                            {
                                                if (localidade == null)
                                                    log.Debug("Municipio não encontrado:" + objLayoutAtualizacao.Municipio);

                                                if (bairro == null)
                                                    log.Debug("Bairro não encontrado:" + objLayoutAtualizacao.BairroInicio);
                                            }

                                            break;

                                        case "U":

                                            if (objLayoutAtualizacao.Logradouro.ToUpper() == "NEDI KALIBABA SPOLAOR")
                                                log.Debug("teste");

                                            if (localidade != null && bairro != null)
                                            {

                                                logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.CepAnterior, context, localidade, bairro);

                                                if (logradouro == null)
                                                    logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Cep, context, localidade, bairro);

                                                if (logradouro != null)
                                                {
                                                    if ((!String.IsNullOrEmpty(logradouro.CEP)) && (!logradouro.CEP.Equals(objLayoutAtualizacao.Cep)))
                                                    {
                                                        log.Debug("Logradouro Atualizado: ");

                                                        log.Debug(logradouro.CODLOGRADOURO);

                                                        log.Debug("Cep Antigo: ");

                                                        log.Debug(logradouro.CEP);

                                                        log.Debug("Cep Novo: ");

                                                        log.Debug(objLayoutAtualizacao.Cep);

                                                        logradouro.CEP = objLayoutAtualizacao.Cep;

                                                    }


                                                    if ((!String.IsNullOrEmpty(logradouro.CEP)) && (!logradouro.TIPO.Equals(objLayoutAtualizacao.TipoLogradouro)))
                                                    {
                                                        log.Debug("Logradouro Atualizado: ");

                                                        log.Debug(logradouro.CODLOGRADOURO);

                                                        log.Debug("Tipo de Logradouro Antigo: ");

                                                        log.Debug(logradouro.TIPO);

                                                        log.Debug("Tipo de logradouro Novo: ");

                                                        log.Debug(objLayoutAtualizacao.TipoLogradouro);

                                                        logradouro.TIPO = objLayoutAtualizacao.TipoLogradouro;

                                                    }

                                                    string nomeLayout = (objLayoutAtualizacao.Preposicao + " " + objLayoutAtualizacao.Logradouro).Trim();

                                                    if (!logradouro.NOME.Equals(nomeLayout))
                                                    {
                                                        log.Debug("Logradouro Atualizado: ");

                                                        log.Debug(logradouro.CODLOGRADOURO);

                                                        log.Debug("Nome Antigo: ");

                                                        log.Debug(logradouro.NOME);

                                                        log.Debug("Nome Novo: ");

                                                        log.Debug(nomeLayout);

                                                        logradouro.NOME = nomeLayout;

                                                    }

                                                }
                                                else
                                                {

                                                    logradouro = new GLOGRADOURO();

                                                    logradouro.CEP = objLayoutAtualizacao.Cep;

                                                    logradouro.CODBAIRROINI = bairro.CODBAIRRO;

                                                    logradouro.CODBAIRROFIM = BuscarBairro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.BairroInicio, context, objLayoutAtualizacao.Municipio).CODBAIRRO;

                                                    logradouro.CODLOCALIDADE = localidade.CODLOCALIDADE;

                                                    logradouro.CODLOGRADOURO = BuscaCodigoLogradouro(context);

                                                    logradouro.NOME = objLayoutAtualizacao.Logradouro;

                                                    logradouro.TIPO = objLayoutAtualizacao.TipoLogradouro;

                                                    logradouro.UF = objLayoutAtualizacao.Uf;

                                                    logradouro.UTILIZATIPO = "S";

                                                    context.GLOGRADOURO.Add(logradouro);

                                                    log.Debug("Codigo Logradouro: ");

                                                    log.Debug(logradouro.CODLOGRADOURO);


                                                }
                                            }
                                            else
                                            {
                                                if (localidade == null)
                                                    log.Debug("Municipio não encontrado:" + objLayoutAtualizacao.Municipio);

                                                if (bairro == null)
                                                    log.Debug("Bairro não encontrado:" + objLayoutAtualizacao.BairroInicio);
                                            }
                                            break;

                                        default:
                                            break;
                                    }

                                    #endregion
                                }
                            }

                            #endregion

                            context.SaveChanges();
                            context.Dispose();
                        }
                        else
                        {
                            #region Arquivo de inclusão

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList();

                            lstLines.RemoveAt(0);

                            List<GLOGRADOURO> lstLogradourosNovos = new List<GLOGRADOURO>();

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    CorporeRM_FabaoEntities context = DefinirContexto();

                                    #region Monta as informações provenientes do arquivo

                                    Layout objLayout = MontarLayout(line);

                                    #endregion

                                    #region Busca o município na GLocalidade

                                    GLOCALIDADE localidade = BuscarLocalidade(objLayout.Uf, objLayout.Municipio, context);

                                    #endregion

                                    #region Busca o bairro na Gbairro

                                    GBAIRRO bairro = BuscarBairro(objLayout.Uf, objLayout.Bairro, context, localidade.NOME);

                                    #endregion

                                    #region Add-Update Logradouros

                                    GLOGRADOURO logradouro = new GLOGRADOURO();

                                    logradouro = BuscaLogradouro(objLayout.Uf, objLayout.Cep, context, localidade, bairro);

                                    if (logradouro == null)
                                    {
                                        logradouro = new GLOGRADOURO();

                                        logradouro.CEP = objLayout.Cep;

                                        logradouro.CODBAIRROINI = bairro.CODBAIRRO;

                                        logradouro.CODLOCALIDADE = localidade.CODLOCALIDADE;

                                        logradouro.CODLOGRADOURO = BuscaCodigoLogradouro(context);

                                        logradouro.NOME = objLayout.Logradouro;

                                        logradouro.TIPO = objLayout.TipoLogradouro;

                                        logradouro.UF = objLayout.Uf;

                                        logradouro.UTILIZATIPO = "S";

                                        context.GLOGRADOURO.Add(logradouro);

                                        log.Debug("Codigo Logradouro: ");

                                        log.Debug(logradouro.CODLOGRADOURO);
                                        
                                    }
                                    else
                                    {
                                        if (!logradouro.TIPO.Equals(objLayout.TipoLogradouro))
                                        {
                                            log.Debug("Logradouro Atualizado: ");

                                            log.Debug(logradouro.CODLOGRADOURO);

                                            log.Debug("Tipo de Logradouro Antigo: ");

                                            log.Debug(logradouro.TIPO);

                                            log.Debug("Tipo de logradouro Novo: ");

                                            log.Debug(objLayout.TipoLogradouro);

                                            logradouro.TIPO = objLayout.TipoLogradouro;

                                        }


                                        if (!String.IsNullOrWhiteSpace(objLayout.Preposicao))
                                        {
                                            string nomeLayout = objLayout.Preposicao + " " + objLayout.Logradouro;

                                            if (!logradouro.NOME.Equals(nomeLayout))
                                            {
                                                log.Debug("Logradouro Atualizado: ");

                                                log.Debug(logradouro.CODLOGRADOURO);

                                                log.Debug("Nome Antigo: ");

                                                log.Debug(logradouro.NOME);

                                                log.Debug("Nome Novo: ");

                                                log.Debug(nomeLayout);

                                                logradouro.NOME = nomeLayout;

                                            }
                                        }
                                    }

                                    context.SaveChanges();

                                    context.Dispose();

                                    #endregion

                                }
                            }

                            #endregion
                        }

                        


                        Console.WriteLine(fileName.ToString() + " processado com sucesso!");

                        

                        //context.Database.Connection.c

                        //SqlConnection.ClearAllPools();

                        #region Remover Arquivo para pasta referente a arquivos já processados
                        
                        //MudarPastaLogradouro(fileName.FullName);

                        #endregion
                        
                    }

                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ocorrido durante o processo de Add-Upp de Logradouro");

                log.Error("Erro ocorrido durante o processo de Add-Upp de Logradouro: ", ex);

                log.Error(ex.InnerException);
            }
        }

        private static void AtualizaGrandeUsuario()
        {
            try
            {
                #region Busca do diretório dos arquivos

                string dir = Environment.CurrentDirectory + @"\ArquivoGrandeUsuario";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                DirectoryInfo infoDir = new DirectoryInfo(dir);

                #endregion

                if (infoDir != null)
                {
                    FileInfo[] fileNames = infoDir.GetFiles("*_GRANDES_USUARIOS.txt");

                    IOrderedEnumerable<FileInfo> arquivosOrdenados = fileNames.OrderBy(f => f.FullName);

                    foreach (FileInfo fileName in arquivosOrdenados)
                    {
                        if (!fileName.FullName.Contains("DNE_DLT_GRANDES_USUARIOS"))
                        {
                            CorporeRM_FabaoEntities context = DefinirContexto();

                            #region Arquivo de Alteração

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList();

                            lstLines.RemoveAt(0);

                            List<GLOGRADOURO> lstLogradourosNovos = new List<GLOGRADOURO>();

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    #region Monta as informações provenientes do arquivo

                                    LayoutAtualizacao objLayoutAtualizacao = MontarLayoutAtualizacao(line);

                                    #endregion

                                    #region Busca o município na GLocalidade

                                    GLOCALIDADE localidade = BuscarLocalidade(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Municipio, context);

                                    #endregion

                                    #region Busca o bairro na Gbairro

                                    GBAIRRO bairro = BuscarBairro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.BairroInicio, context, localidade.NOME);

                                    #endregion

                                    #region Add-Update-Delete Logradouros

                                    GLOGRADOURO logradouro = new GLOGRADOURO();

                                    Console.WriteLine("Logradouro: " + objLayoutAtualizacao.Logradouro);

                                    switch (objLayoutAtualizacao.Operacao)
                                    {
                                        case "D":

                                            //logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Cep, context, localidade, bairro);

                                            //if (logradouro != null)
                                            //    context.GLOGRADOURO.Remove(logradouro);
                                            break;

                                        case "I":


                                            if (localidade != null && bairro != null)
                                            {

                                                logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Cep, context, localidade, bairro);

                                                if (logradouro == null)
                                                {
                                                    logradouro = new GLOGRADOURO();

                                                    logradouro.CEP = objLayoutAtualizacao.Cep;

                                                    logradouro.CODBAIRROINI = bairro.CODBAIRRO;

                                                    logradouro.CODBAIRROFIM = bairro.CODBAIRRO; //BuscarBairro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.BairroFim, context, objLayoutAtualizacao.Municipio).CODBAIRRO;

                                                    logradouro.CODLOCALIDADE = localidade.CODLOCALIDADE;

                                                    logradouro.CODLOGRADOURO = BuscaCodigoLogradouro(context);

                                                    logradouro.NOME = objLayoutAtualizacao.Logradouro;

                                                    logradouro.TIPO = objLayoutAtualizacao.TipoLogradouro;

                                                    logradouro.UF = objLayoutAtualizacao.Uf;

                                                    logradouro.UTILIZATIPO = "S";

                                                    context.GLOGRADOURO.Add(logradouro);

                                                    log.Debug("Codigo Logradouro: ");

                                                    log.Debug(logradouro.CODLOGRADOURO);

                                                }
                                            }
                                            else
                                            {
                                                if (localidade == null)
                                                    log.Debug("Municipio não encontrado:" + objLayoutAtualizacao.Municipio);

                                                if (bairro == null)
                                                    log.Debug("Bairro não encontrado:" + objLayoutAtualizacao.BairroInicio);
                                            }

                                            break;

                                        case "U":

                                            if (objLayoutAtualizacao.Logradouro.ToUpper() == "NEDI KALIBABA SPOLAOR")
                                                log.Debug("teste");

                                            if (localidade != null && bairro != null)
                                            {

                                                logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.CepAnterior, context, localidade, bairro);

                                                if (logradouro == null)
                                                    logradouro = BuscaLogradouro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.Cep, context, localidade, bairro);

                                                if (logradouro != null)
                                                {
                                                    if ((!String.IsNullOrEmpty(logradouro.CEP)) && (!logradouro.CEP.Equals(objLayoutAtualizacao.Cep)))
                                                    {
                                                        log.Debug("Logradouro Atualizado: ");

                                                        log.Debug(logradouro.CODLOGRADOURO);

                                                        log.Debug("Cep Antigo: ");

                                                        log.Debug(logradouro.CEP);

                                                        log.Debug("Cep Novo: ");

                                                        log.Debug(objLayoutAtualizacao.Cep);

                                                        logradouro.CEP = objLayoutAtualizacao.Cep;

                                                    }


                                                    if ((!String.IsNullOrEmpty(logradouro.CEP)) && (!logradouro.TIPO.Equals(objLayoutAtualizacao.TipoLogradouro)))
                                                    {
                                                        log.Debug("Logradouro Atualizado: ");

                                                        log.Debug(logradouro.CODLOGRADOURO);

                                                        log.Debug("Tipo de Logradouro Antigo: ");

                                                        log.Debug(logradouro.TIPO);

                                                        log.Debug("Tipo de logradouro Novo: ");

                                                        log.Debug(objLayoutAtualizacao.TipoLogradouro);

                                                        logradouro.TIPO = objLayoutAtualizacao.TipoLogradouro;

                                                    }

                                                    string nomeLayout = (objLayoutAtualizacao.Preposicao + " " + objLayoutAtualizacao.Logradouro).Trim();

                                                    if (!logradouro.NOME.Equals(nomeLayout))
                                                    {
                                                        log.Debug("Logradouro Atualizado: ");

                                                        log.Debug(logradouro.CODLOGRADOURO);

                                                        log.Debug("Nome Antigo: ");

                                                        log.Debug(logradouro.NOME);

                                                        log.Debug("Nome Novo: ");

                                                        log.Debug(nomeLayout);

                                                        logradouro.NOME = nomeLayout;

                                                    }

                                                }
                                                else
                                                {

                                                    logradouro = new GLOGRADOURO();

                                                    logradouro.CEP = objLayoutAtualizacao.Cep;

                                                    logradouro.CODBAIRROINI = bairro.CODBAIRRO;

                                                    logradouro.CODBAIRROFIM = BuscarBairro(objLayoutAtualizacao.Uf, objLayoutAtualizacao.BairroInicio, context, objLayoutAtualizacao.Municipio).CODBAIRRO;

                                                    logradouro.CODLOCALIDADE = localidade.CODLOCALIDADE;

                                                    logradouro.CODLOGRADOURO = BuscaCodigoLogradouro(context);

                                                    logradouro.NOME = objLayoutAtualizacao.Logradouro;

                                                    logradouro.TIPO = objLayoutAtualizacao.TipoLogradouro;

                                                    logradouro.UF = objLayoutAtualizacao.Uf;

                                                    logradouro.UTILIZATIPO = "S";

                                                    context.GLOGRADOURO.Add(logradouro);

                                                    log.Debug("Codigo Logradouro: ");

                                                    log.Debug(logradouro.CODLOGRADOURO);


                                                }
                                            }
                                            else
                                            {
                                                if (localidade == null)
                                                    log.Debug("Municipio não encontrado:" + objLayoutAtualizacao.Municipio);

                                                if (bairro == null)
                                                    log.Debug("Bairro não encontrado:" + objLayoutAtualizacao.BairroInicio);
                                            }
                                            break;

                                        default:
                                            break;
                                    }

                                    #endregion
                                }
                            }

                            #endregion

                            context.SaveChanges();
                            context.Dispose();
                        }
                        else
                        {
                            #region Arquivo de inclusão

                            List<string> lstLines = File.ReadAllLines(fileName.FullName, Encoding.GetEncoding("iso-8859-1")).ToList();

                            lstLines.RemoveAt(0);

                            List<GLOGRADOURO> lstLogradourosNovos = new List<GLOGRADOURO>();

                            foreach (string line in lstLines)
                            {
                                if (!line[0].Equals('#'))
                                {
                                    CorporeRM_FabaoEntities context = DefinirContexto();

                                    #region Monta as informações provenientes do arquivo

                                    Layout objLayout = MontarLayoutGrandeUsuario(line);

                                    #endregion

                                    #region Busca o município na GLocalidade

                                    GLOCALIDADE localidade = BuscarLocalidade(objLayout.Uf, objLayout.Municipio, context);

                                    #endregion

                                    #region Busca o bairro na Gbairro

                                    GBAIRRO bairro = BuscarBairro(objLayout.Uf, objLayout.Bairro, context, localidade.NOME);

                                    #endregion

                                    #region Add-Update Logradouros

                                    GLOGRADOURO logradouro = new GLOGRADOURO();

                                    logradouro = BuscaLogradouro(objLayout.Uf, objLayout.Cep, context, localidade, bairro);

                                    if (logradouro == null)
                                    {
                                        logradouro = new GLOGRADOURO();

                                        logradouro.CEP = objLayout.Cep;

                                        logradouro.CODBAIRROINI = bairro.CODBAIRRO;

                                        logradouro.CODLOCALIDADE = localidade.CODLOCALIDADE;

                                        logradouro.CODLOGRADOURO = BuscaCodigoLogradouro(context);

                                        logradouro.NOME = objLayout.Logradouro;

                                        logradouro.TIPO = objLayout.TipoLogradouro;

                                        logradouro.UF = objLayout.Uf;

                                        logradouro.UTILIZATIPO = "S";

                                        context.GLOGRADOURO.Add(logradouro);

                                        log.Debug("Codigo Logradouro: ");

                                        log.Debug(logradouro.CODLOGRADOURO);

                                    }
                                    else
                                    {
                                        if (!logradouro.TIPO.Equals(objLayout.TipoLogradouro))
                                        {
                                            log.Debug("Logradouro Atualizado: ");

                                            log.Debug(logradouro.CODLOGRADOURO);

                                            log.Debug("Tipo de Logradouro Antigo: ");

                                            log.Debug(logradouro.TIPO);

                                            log.Debug("Tipo de logradouro Novo: ");

                                            log.Debug(objLayout.TipoLogradouro);

                                            logradouro.TIPO = objLayout.TipoLogradouro;

                                        }


                                        if (!String.IsNullOrWhiteSpace(objLayout.Preposicao))
                                        {
                                            string nomeLayout = objLayout.Preposicao + " " + objLayout.Logradouro;

                                            if (!logradouro.NOME.Equals(nomeLayout))
                                            {
                                                log.Debug("Logradouro Atualizado: ");

                                                log.Debug(logradouro.CODLOGRADOURO);

                                                log.Debug("Nome Antigo: ");

                                                log.Debug(logradouro.NOME);

                                                log.Debug("Nome Novo: ");

                                                log.Debug(nomeLayout);

                                                logradouro.NOME = nomeLayout;

                                            }
                                        }
                                    }

                                    context.SaveChanges();

                                    context.Dispose();

                                    #endregion

                                }
                            }

                            #endregion
                        }




                        Console.WriteLine(fileName.ToString() + " processado com sucesso!");



                        //context.Database.Connection.c

                        //SqlConnection.ClearAllPools();

                        #region Remover Arquivo para pasta referente a arquivos já processados

                        //MudarPastaLogradouro(fileName.FullName);

                        #endregion

                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ocorrido durante o processo de Add-Upp de Logradouro");

                log.Error("Erro ocorrido durante o processo de Add-Upp de Logradouro: ", ex);

                log.Error(ex.InnerException);
            }
        }

        #endregion

        #region Métodos de Busca

        private static GLOGRADOURO BuscaLogradouro(string uf, string cep, CorporeRM_FabaoEntities context, GLOCALIDADE localidade, GBAIRRO bairro)
        {
            IQueryable<GLOGRADOURO> logradouro = from lo in context.GLOGRADOURO
                                             where lo.UF == uf 
                                             && lo.CODLOCALIDADE == localidade.CODLOCALIDADE 
                                             && lo.CEP == cep
                                             select lo;

            return logradouro.FirstOrDefault();

        }

        private static GBAIRRO BuscarBairro(string uf, string nomeBairro, CorporeRM_FabaoEntities context, string localidade)
        {
            IQueryable<GBAIRRO> bairro = from b in context.GBAIRRO
                                         where b.UF == uf && b.GLOCALIDADE.NOME == localidade && b.NOME == nomeBairro
                                         select b;
            
            return bairro.FirstOrDefault();
        }

        private static GLOCALIDADE BuscarLocalidade(string uf, string municipio, CorporeRM_FabaoEntities context, string cep)
        {

            IQueryable<GLOCALIDADE> local = from l in context.GLOCALIDADE
                                            where l.UF == uf && l.NOME == municipio 
                                            select l;

            if (!String.IsNullOrEmpty(cep))
                local = local.Where(l => l.CEP.Equals(cep));

            return local.FirstOrDefault();
            
        }


        #region Preenchimento de Layout

        #region Layout para arquivos de inclusao

        private static Layout MontarLayout(string line)
        {
            Layout objLayout = new Layout();

            objLayout.Uf = line.Substring(1, 2).Trim();

            objLayout.Municipio = line.Substring(17,77).Trim();

            objLayout.Bairro = line.Substring(102,77).Trim();

            objLayout.TipoLogradouro = line.Substring(259, 26).Trim();

            objLayout.Preposicao = line.Substring(285, 3).Trim();

            objLayout.Logradouro = line.Substring(374, 71).Trim();

            objLayout.Cep = line.Substring(518, 8).Trim();

            return objLayout;
        }

        private static Layout MontarLayoutGrandeUsuario(string line)
        {
            Layout objLayout = new Layout();

            objLayout.Uf = line.Substring(1, 2).Trim();

            objLayout.Municipio = line.Substring(17, 77).Trim();

            objLayout.Bairro = line.Substring(102, 77).Trim();

            objLayout.TipoLogradouro = string.Empty;

            objLayout.Preposicao = string.Empty;

            objLayout.Logradouro = line.Substring(188, 72).Trim();

            objLayout.Cep = line.Substring(260, 8).Trim();

            return objLayout;
        }

        private static LayoutBairro MontarLayoutBairro(string line)
        {
            LayoutBairro objLayoutBairro = new LayoutBairro();

            objLayoutBairro.Uf = line.Substring(1, 8).Trim();

            objLayoutBairro.Municipio = line.Substring(17, 77).Trim();

            objLayoutBairro.Bairro = line.Substring(102, 72).Trim();

            objLayoutBairro.NomeAbreviado = line.Substring(174, 36).Trim();

            return objLayoutBairro;
        }

        private static LayoutLocalidade MontarLayoutLocalidade(string line)
        {
            LayoutLocalidade objLayoutLocalidade = new LayoutLocalidade();

            objLayoutLocalidade.Uf = line.Substring(3, 2).Trim();

            objLayoutLocalidade.Municipio = line.Substring(19, 72).Trim();

            objLayoutLocalidade.Cep = line.Substring(91, 8).Trim();

            return objLayoutLocalidade;

        }

        #endregion

        #region Layout para arquivos de alteração

        private static LayoutAtualizacaoLocalidade MontarLayoutAlteracaoLocalidade(string line)
        {
            LayoutAtualizacaoLocalidade objLayoutAtualizacaoLocalidade = new LayoutAtualizacaoLocalidade();

            objLayoutAtualizacaoLocalidade.Uf = line.Substring(3, 2).Trim();

            objLayoutAtualizacaoLocalidade.Municipio = line.Substring(19, 72).Trim();

            objLayoutAtualizacaoLocalidade.Cep = line.Substring(91, 8).Trim();

            objLayoutAtualizacaoLocalidade.Tipo = line.Substring(135, 1).Trim();

            objLayoutAtualizacaoLocalidade.Operacao = line.Substring(161,1).Trim();

            objLayoutAtualizacaoLocalidade.CepAnterior = line.Substring(162, 8).Trim();

            return objLayoutAtualizacaoLocalidade;
        }

        private static LayoutAtualizacaoBairro MontarLayoutAlteracaoBairro(string line)
        {
            LayoutAtualizacaoBairro objLayoutAtualizacaoBairro = new LayoutAtualizacaoBairro();

            objLayoutAtualizacaoBairro.Uf = line.Substring(1, 2).Trim();

            objLayoutAtualizacaoBairro.Municipio = line.Substring(17, 72).Trim();

            objLayoutAtualizacaoBairro.Bairro = line.Substring(102, 72).Trim();

            objLayoutAtualizacaoBairro.NomeAbreviado = line.Substring(174, 36).Trim();

            objLayoutAtualizacaoBairro.Operacao = line.Substring(210,1).Trim();

            return objLayoutAtualizacaoBairro;
        }

        private static LayoutAtualizacao MontarLayoutAtualizacao(string line)
        {
            LayoutAtualizacao objLayoutAtualizacao = new LayoutAtualizacao();

            objLayoutAtualizacao.Uf = line.Substring(1,2).Trim();

            objLayoutAtualizacao.Municipio = line.Substring(17,72).Trim();

            objLayoutAtualizacao.BairroInicio = line.Substring(102,72).Trim();

            objLayoutAtualizacao.BairroFim = line.Substring(187, 72).Trim();

            objLayoutAtualizacao.TipoLogradouro = line.Substring(259, 26).Trim();

            objLayoutAtualizacao.Preposicao = line.Substring(285, 3);

            objLayoutAtualizacao.Logradouro = line.Substring(374,72).Trim();

            objLayoutAtualizacao.Cep = line.Substring(518,8).Trim();

            objLayoutAtualizacao.Operacao = line.Substring(528, 1).Trim();

            objLayoutAtualizacao.CepAnterior = line.Substring(529,8).Trim();
            
            return objLayoutAtualizacao;
        }

        #endregion

        #endregion

        #endregion

        #region Métodos de Geração de código

        private static int BuscaCodigoLocalidade(CorporeRM_FabaoEntities context)
        {
            if (proxCodigoLocalidade.Equals(0))
            {
                //var listaTeste = (from l in context.GLOCALIDADE select l).Max(l => l.CODLOCALIDADE);

                proxCodigoLocalidade = (from l in context.GLOCALIDADE select l).Max(l => l.CODLOCALIDADE);
                proxCodigoLocalidade++;
            }
            else
                proxCodigoLocalidade++;

            return proxCodigoLocalidade;
             
        }

        private static GLOCALIDADE BuscarLocalidade(string uf, string localidade, CorporeRM_FabaoEntities context)
        {
            IQueryable<GLOCALIDADE> local = from l in context.GLOCALIDADE
                                            where l.UF == uf && l.NOME == localidade
                                            select l;
            return local.FirstOrDefault();
        }

        private static int BuscaCodigoLogradouro(CorporeRM_FabaoEntities context)
        {
            if (proxCodigoLogradouro.Equals(0))
            {
                proxCodigoLogradouro = (from l in context.GLOGRADOURO select l.CODLOGRADOURO).Max() + 1;
                return proxCodigoLogradouro;
            }
            else
            {
                proxCodigoLogradouro++;
                return proxCodigoLogradouro;
            } 

        }

        private static int BuscarCodigoBairro(CorporeRM_FabaoEntities context)
        {

            if (proxCodigoBairro.Equals(0))
            {
                proxCodigoBairro = (from l in context.GBAIRRO select l.CODBAIRRO).Max() + 1;
                return proxCodigoBairro;
            }
            else
            {
                proxCodigoBairro++;
                return proxCodigoBairro;
            }
        }

        #endregion

        // Ainda ha ser complementado
        #region Métodos de Mudança de pastas

        public static void MudarPastaLocalidade(FileInfo file)
        {
            string dir = Environment.CurrentDirectory + @"\ArquivoLocalidadeProcessado";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            

            Directory.Move(file.DirectoryName, dir);
        }

        public static void MudarPastaBairro(string fullName)
        {
            string dir = Environment.CurrentDirectory + @"\ArquivoBairroProcessado";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Directory.Move(fullName, dir + @"\" + fullName);
        }

        public static void MudarPastaLogradouro(string fullName)
        {
            string dir = Environment.CurrentDirectory + @"\ArquivoLogradouroProcessado";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Directory.Move(fullName, dir + @"\" + fullName);
        }

        #endregion
    }

}
