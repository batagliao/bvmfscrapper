using System;
namespace bvmfscrapper.models
{
    public enum SegmentoEnum
    {
        NovoMercado,
        Nivel1Governanca,
        Nivel2Governanca,
        BovespaMais,
        BovespaMaisNivel2,
        BalcaoOrganizadoTradicional,
        BDRNivel1,
        BDRNivel2,
        BDRNivel3,
        BDRNaoPatrocinado,
        NaoEspecificado

        /*(NM) Cia. Novo Mercado
		(N1) Cia. Nível 1 de Governança Corporativa
		(N2) Cia. Nível 2 de Governança Corporativa
		(MA) Cia. Bovespa Mais
		(M2) Cia. Bovespa Mais Nível 2
		(MB) Cia. Balcão Org. Tradicional
		(DR1) BDR Nível 1
		(DR2) BDR Nível 2
		(DR3) BDR Nível 3
		(DRN) BDR Não Patrocinado
        */
    }

    public static class SegmentoEnumExtensions
    {
        public static string AsString(this SegmentoEnum segmento)
        {
            switch (segmento)
            {
                case SegmentoEnum.NovoMercado:
                    return "NM";
                case SegmentoEnum.Nivel1Governanca:
                    return "N1";
                case SegmentoEnum.Nivel2Governanca:
                    return "N2";
                case SegmentoEnum.BovespaMais:
                    return "MA";
                case SegmentoEnum.BovespaMaisNivel2:
                    return "M2";
                case SegmentoEnum.BalcaoOrganizadoTradicional:
                    return "MB";
                case SegmentoEnum.BDRNivel1:
                    return "DR1";
                case SegmentoEnum.BDRNivel2:
                    return "DR2";
                case SegmentoEnum.BDRNivel3:
                    return "DR3";
                case SegmentoEnum.BDRNaoPatrocinado:
                    return "DRN";
                default:
                    return "";
            }
        }

        public static SegmentoEnum FromString(string text)
        {
            switch (text)
            {
                case "NM":
                    return SegmentoEnum.NovoMercado;
                case "N1":
                    return SegmentoEnum.Nivel1Governanca;
                case "N2":
                    return SegmentoEnum.Nivel2Governanca;
                case "MA":
                    return SegmentoEnum.BovespaMais;
                case "M2":
                    return SegmentoEnum.BovespaMaisNivel2;
                case "MB":
                    return SegmentoEnum.BalcaoOrganizadoTradicional;
                case "DR1":
                    return SegmentoEnum.BDRNivel1;
                case "DR2":
                    return SegmentoEnum.BDRNivel2;
                case "DR3":
                    return SegmentoEnum.BDRNivel3;
                case "DRN":
                    return SegmentoEnum.BDRNaoPatrocinado;
                default:
                    return SegmentoEnum.NaoEspecificado;
            }
        }
    }



}
