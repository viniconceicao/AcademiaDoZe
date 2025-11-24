// Aluno: Vinicius de Liz da Conceição
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
                LaudoMedico = matricula.LaudoMedico != null ? new ArquivoDTO 
                { 
                    Conteudo = matricula.LaudoMedico.Conteudo,
                    ContentType = ".pdf" 
                } : null,
            };
        }
        
        public static Matricula ToEntity(this MatriculaDTO matriculaDto)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.ToEntity - Início");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.ToEntity - MatriculaDTO.Id: {matriculaDto.Id}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.ToEntity - Aluno DTO ID: {matriculaDto.AlunoMatricula.Id}");
            
            var alunoEntity = matriculaDto.AlunoMatricula.ToEntityMatricula();
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.ToEntity - Aluno Entity ID: {alunoEntity.Id}");
            
            var matricula = Matricula.Criar(
                alunoEntity,
                matriculaDto.Plano.ToDomain(),
                matriculaDto.DataInicio,
                matriculaDto.DataFim,
                matriculaDto.Objetivo,
                matriculaDto.RestricoesMedicas.ToDomain(),
                (matriculaDto.LaudoMedico?.Conteudo != null && matriculaDto.LaudoMedico?.ContentType != null)
                    ? Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo, matriculaDto.LaudoMedico.ContentType)
                    : null,
                matriculaDto.ObservacoesRestricoes ?? string.Empty
            );
            
            // ✅ IMPORTANTE: Preserva o ID do DTO na entidade criada
            if (matriculaDto.Id > 0)
            if (matriculaDto.Id > 0)
            {
                typeof(Entity).GetProperty("Id")?.SetValue(matricula, matriculaDto.Id);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.ToEntity - ID preservado: {matricula.Id}");
            }
            
            return matricula;
        }
        
        public static Matricula UpdateFromDto(this Matricula matriculaExistente, MatriculaDTO matriculaDto)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.UpdateFromDto - Início");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.UpdateFromDto - MatriculaExistente.Id: {matriculaExistente.Id}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.UpdateFromDto - MatriculaDTO.Id: {matriculaDto.Id}");
            
            var matriculaAtualizada = Matricula.Criar(
                matriculaDto.AlunoMatricula.ToEntityMatricula() ?? matriculaExistente.AlunoMatricula,
                matriculaDto.Plano != default ? matriculaDto.Plano.ToDomain() : matriculaExistente.Plano,
                matriculaDto.DataInicio != default ? matriculaDto.DataInicio : matriculaExistente.DataInicio,
                matriculaDto.DataFim != default ? matriculaDto.DataFim : matriculaExistente.DataFim,
                matriculaDto.Objetivo ?? matriculaExistente.Objetivo,
                matriculaDto.RestricoesMedicas != default ? matriculaDto.RestricoesMedicas.ToDomain() : matriculaExistente.RestricoesMedicas,
                (matriculaDto.LaudoMedico?.Conteudo != null && matriculaDto.LaudoMedico?.ContentType != null)
                    ? Arquivo.Criar(matriculaDto.LaudoMedico.Conteudo, matriculaDto.LaudoMedico.ContentType)
                    : matriculaExistente.LaudoMedico,
                matriculaDto.ObservacoesRestricoes ?? matriculaExistente.ObservacoesRestricoes
            );
            
            // ✅ CRÍTICO: Preserva o ID da matrícula existente
            typeof(Entity).GetProperty("Id")?.SetValue(matriculaAtualizada, matriculaExistente.Id);
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaMappings.UpdateFromDto - ID preservado: {matriculaAtualizada.Id}");
            
            return matriculaAtualizada;
        }
    }
}