using App.Application.Dto;
using App.Domain.Model;
using AutoMapper;

namespace App.Application.MappingProfiles;

public class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        // Project -> ProjectDto
        CreateMap<Project, ProjectDto>();

        // Project -> ProjectListDto
        CreateMap<Project, ProjectListDto>();

        // Project -> ProjectDetailsDto
        CreateMap<Project, ProjectDetailsDto>()
            .ForMember(dest => dest.WorkspaceName, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.Ignore());

        // ProjectMember -> ProjectMemberDto
        CreateMap<ProjectMember, ProjectMemberDto>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.UserEmail, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role));

        // CreateProjectDto -> Project
        CreateMap<CreateProjectDto, Project>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsArchived, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.Ignore());
    }
}
