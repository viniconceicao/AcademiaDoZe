// Aluno: Vinicius de Liz da Conceição
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
namespace AcademiaDoZe.Domain.Enums
{
    public enum EColaboradorTipo
    {
        [Display(Name = "Colaborador")]
        Colaborador = 0,
        [Display(Name = "Aluno")]
        Aluno = 1,
        [Display(Name = "Administrador")]
        Administrador = 2
    }
}