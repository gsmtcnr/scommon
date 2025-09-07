using scommon.Auths;

namespace scommon_sample_web_api.Auths;

public static class AuthExtensions
{
    public static void LoadCurrentUser(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<ICurrentUser>(provider =>
        {
            //get from jwt
            return new CurrentUser
            {
                Id = Guid.NewGuid(),
                Name = "Samet Ç.",
                Email = "sametcinar@msn.com",
                Phone = "544 999 99 99"
            };
        });
    }
}
