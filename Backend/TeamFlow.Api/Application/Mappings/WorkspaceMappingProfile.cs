using App.Application.Dto;
using App.Domain.Model;
using AutoMapper;

namespace App.Application.MappingProfiles;

public class WorkspaceMappingProfile : Profile
{
    public WorkspaceMappingProfile()
    {
        // Workspace -> WorkspaceListDto
        CreateMap<Workspace, WorkspaceListDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectCount, opt => opt.Ignore());

        // Workspace -> WorkspaceDetailsDto
        CreateMap<Workspace, WorkspaceDetailsDto>()
            .ForMember(dest => dest.OwnerName, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.Ignore())
            .ForMember(dest => dest.Projects, opt => opt.Ignore());

        // WorkspaceMember -> WorkspaceMemberDto
        CreateMap<WorkspaceMember, WorkspaceMemberDto>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore())
            .ForMember(dest => dest.UserEmail, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role));

        // Project -> ProjectSummaryDto
        CreateMap<Project, ProjectSummaryDto>();

        // CreateWorkspaceDto -> Workspace
        CreateMap<CreateWorkspaceDto, Workspace>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.IsArchived, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Members, opt => opt.Ignore())
            .ForMember(dest => dest.Projects, opt => opt.Ignore());
    }
}
