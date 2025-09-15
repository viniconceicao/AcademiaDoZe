﻿// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Application.Mappings
{
    public static class MatriculaMappings
    {
        public static MatriculaDTO ToDto(this Matricula matricula)
        {
            return new MatriculaDTO
            {
                Id = matricula.Id,
                AlunoMatricula = matricula.AlunoMatricula.ToDto(),
                Plano = matricula.Plano.ToApp(),
                DataInicio = matricula.DataInicio,
                DataFim = matricula.DataFim,
                Objetivo = matricula.Objetivo,
                RestricoesMedicas = matricula.RestricoesMedicas.ToApp(),
                ObservacoesRestricoes = matricula.ObservacoesRestricoes,
                LaudoMedico = matricula.LaudoMedico != null ? new ArquivoDTO { Conteudo = matricula.LaudoMedico.Conteudo } : null, // Mapeia laudo para DTO
            };
        }
        public static Matricula ToEntity(this MatriculaDTO matriculaDto)
        {
            return Matricula.Criar(
                matriculaDto.AlunoMatricula.ToEntityMatricula(), // Mapeia aluno do DTO para a entidade, resolvendo o caso da senha null
                matriculaDto.Plano.ToDomain(),
                matriculaDto.DataInicio,
                matriculaDto.DataFim,
                matriculaDto.Objetivo,
                matriculaDto.RestricoesMedicas.ToDomain(),
                (matriculaDto.LaudoMedico?.Conteudo != null && matriculaDto.LaudoMedico?.ContentType != null)
                    ? Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo, matriculaDto.LaudoMedico.ContentType)
                    : null, // Mapeia laudo do DTO para a entidade
                matriculaDto.ObservacoesRestricoes!
            );
        }
        public static Matricula UpdateFromDto(this Matricula matricula, MatriculaDTO matriculaDto)
        {
            return Matricula.Criar(
                matriculaDto.AlunoMatricula.ToEntityMatricula() ?? matricula.AlunoMatricula,
                matriculaDto.Plano != default ? matriculaDto.Plano.ToDomain() : matricula.Plano,
                matriculaDto.DataInicio != default ? matriculaDto.DataInicio : matricula.DataInicio,
                matriculaDto.DataFim != default ? matriculaDto.DataFim : matricula.DataFim,
                matriculaDto.Objetivo ?? matricula.Objetivo,
                matriculaDto.RestricoesMedicas != default ? matriculaDto.RestricoesMedicas.ToDomain() : matricula.RestricoesMedicas,
                (matriculaDto.LaudoMedico?.Conteudo != null && matriculaDto.LaudoMedico?.ContentType != null)
                    ? Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo, matriculaDto.LaudoMedico.ContentType)
                    : matricula.LaudoMedico, // Atualiza laudo se fornecido
                matriculaDto.ObservacoesRestricoes ?? matricula.ObservacoesRestricoes
            );
        }
    }
}