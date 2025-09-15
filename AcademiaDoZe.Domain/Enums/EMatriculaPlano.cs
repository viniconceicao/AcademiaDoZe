﻿// Aluno: Vinicius de Liz da Conceição
using System.ComponentModel.DataAnnotations;
namespace AcademiaDoZe.Domain.Enums
{
    public enum EMatriculaPlano
    {
        [Display(Name = "Mensal")]
        Mensal = 0,
        [Display(Name = "Trimestral")]
        Trimestral = 1,
        [Display(Name = "Semestral")]
        Semestral = 2,
        [Display(Name = "Anual")]
        Anual = 3
    }
}