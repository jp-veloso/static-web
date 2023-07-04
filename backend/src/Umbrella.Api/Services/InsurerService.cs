using Microsoft.EntityFrameworkCore;
using Umbrella.Api.Contexts;
using Umbrella.Api.Dtos;
using Umbrella.Api.Entities;
using Umbrella.Api.Entities.Enums;
using Umbrella.Api.Resources.Exceptions;
using Umbrella.Api.Services.Exceptions;

namespace Umbrella.Api.Services;

public class InsurerService
{
    public List<InsurerDTO> FindAll()
    {
        using RepositoryContext db = new();
        return db.Insurers.Where(x => x.Active).OrderBy(x => x.Name).Select(x => new InsurerDTO(x)).ToList();
    }

    public List<InsurerDTO> FindAllWithParameters(ProposalType type)
    {
        using RepositoryContext db = new();
        
        List<InsurerDTO> insurers = db.Insurers.Include(x => x.ProposalParameters)
            .Where(x => x.Active && x.ProposalParameters.Count > 0).OrderBy(x => x.Name)
            .Select(x => new InsurerDTO(x, x.ProposalParameters, type)).ToList();

        return insurers;
    }

    public InsurerDTO FindByIdWithParameters(int id, ProposalType type)
    {
        using RepositoryContext db = new();

        Insurer? insurer = db.Insurers.Include(x => x.ProposalParameters)
            .SingleOrDefault(x => x.Id == id && x.Active && x.ProposalParameters.Count > 0);

        if (insurer == null)
        {
            throw new ServiceException("Insurer not found or without parameters", new StandardError
            {
                Error = "Insurer not found or without parameters",
                Message = $"Entity with insurer_id = {id} not found or don't have any parameters",
                Status = 404
            });
        }

        return new InsurerDTO(insurer, insurer.ProposalParameters, type);
    }

    public InsurerDTO FindById(int id)
    {
        using RepositoryContext db = new();

        Insurer? insurer = db.Insurers.Find(id);

        if (insurer == null)
        {
            throw new ServiceException("Insurer not found", new StandardError
                                                            {
                                                                Error = "Insurer not found",
                                                                Message = $"Entity with insurer_id = {id} not found",
                                                                Status = 404
                                                            });
        }

        return new InsurerDTO(insurer);
    }

    public void UpdateParameters(int id, ParametersDTO data)
    {
        using RepositoryContext db = new();

        Insurer? insurer = db.Insurers
            .Include(x => x.ProposalParameters)
            .SingleOrDefault(x => x.Id == id);

        if (insurer == null)
        {
            throw new ServiceException("Insurer not found", new StandardError
            {
                Error = "Insurer not found",
                Message = $"Entity with insurer_id = {id} not found",
                Status = 404
            });
        }

        var parameters = insurer.ProposalParameters.SingleOrDefault(x => x.ProposalType == data.ProposalType);

        if (parameters == null)
        {
            throw new ServiceException("Parameters not found", new StandardError
            {
                Error = "Parameters not found",
                Message = $"Entity with insurer_id = {id} and contract_type = {data.ProposalType} not found",
                Status = 404
            });
        }

        insurer.ProposalParameters.Remove(parameters);

        parameters.BaseCommission = data.BaseCommission;
        parameters.MaximumCommission = data.MaximumCommission;
        parameters.Ccg = data.Ccg;
        parameters.MinimumBounty = data.MinimumBounty;
        parameters.MinimumBrokerage = data.MinimumBrokerage;
        parameters.ExternalRetroactivity = data.ExternalRetroactivity;
        parameters.InternalRetroactivity = data.InternalRetroactivity;
        parameters.Pstp = data.Pstp;
        parameters.Exclusive = data.Exclusive;
        
        insurer.ProposalParameters.Add(parameters);

        db.SaveChanges();
    }
}