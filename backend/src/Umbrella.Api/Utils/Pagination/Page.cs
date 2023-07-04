namespace Umbrella.Api.Utils.Pagination;

public class Page<T> where T : class
{
    public IEnumerable<T> Data { get; }

    public int TotalPages { get; set; }
    public int TotalElements { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public Page(IEnumerable<T> data)
    {
        Data = data;
    }

    public Page<R> Convert<R>(Func<T, R> converter) where R : class
    {
        List<R> converted = Data.Select(converter.Invoke)
                                .ToList();

        return new Page<R>(converted)
               {
                   PageNumber = PageNumber,
                   PageSize = PageSize,
                   TotalPages = TotalPages,
                   TotalElements = TotalElements
               };
    }
}