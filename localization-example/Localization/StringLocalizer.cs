using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace localization_example.Localization
{
    public class StringLocalizer<T> : IStringLocalizer<T>
    {
        public IDistributedCache Cache { get; }

        public LocalizedString this[string name]
        {
            get
            {
                var translation = GetOrAddTranslation(name);
                return new LocalizedString(name, translation ?? name, translation != null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var translation = GetOrAddTranslation(name);

                if (translation is not null)
                {
                    translation = string.Format(translation, arguments);
                }
                return new LocalizedString(name, translation ?? name, translation != null);
            }
        }

        public string ConnectionString { get; set; }
        public StringLocalizer(IDistributedCache cache, IConfiguration configuration)
        {
            Cache = cache;

            ConnectionString = configuration.GetConnectionString("Default") ?? throw new NullReferenceException("No ConnectionString found for Default");
        }

        string GetOrAddTranslation(string name)
        {
            var culture = CultureInfo.CurrentUICulture;
            string key = $"localization:{culture.Name}:{name}";

            string? translation = Cache.GetString(key);

            // Translation not found? Get it from the database and cache it
            if (translation is null)
            {
                using (var connection = new SqlConnection(this.ConnectionString))
                {
                    translation = connection.ExecuteScalar<string>(
                    sql: @"SELECT [Text]
                            FROM [Local].[Translation]
                            INNER JOIN [Local].[Language] ON [Language].[LanguageId] = [Translation].[LanguageId]
                            WHERE [Language].[Code2] = @Code2 AND [Translation].[Key] = @Key",
                    param: new
                    {
                        Code2 = culture.Name,
                        Key = name
                    });

                    if (translation is not null)
                    {
                        var options = new DistributedCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                            .SetAbsoluteExpiration(DateTime.Now.AddHours(6));
                        Cache.SetString(key, translation, options);
                    }
                }
            }

            // return key if translation not found
            return translation ?? name;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentCulture;
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                var resources = connection.Query<Resource>(
                    sql: @"SELECT [TranslationId]
                                        ,[Key]
                                        ,[Text]
                                    FROM [Local].[Translation]
                                    INNER JOIN [Local].[Language] ON [Language].[LanguageId] = [Translation].[LanguageId]
                                    WHERE [Language].[Code2] = @Code2",
                    param: new
                    {
                        Code2 = culture.TwoLetterISOLanguageName,
                    });
                foreach (var resource in resources)
                {
                    yield return new LocalizedString(
                        name: resource.Key,
                        value: resource.Text,
                        resourceNotFound: !String.IsNullOrWhiteSpace(resource.Text));
                }
            }
        }
    }
}
