using Sitecore.Buckets.Managers;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Providers.Sc.DataAccess.Readers;
using Sitecore.DataExchange.Providers.Sc.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.Services.Core.Diagnostics;
using Sitecore.Services.Core.Model;
using Sitecore.Services.Infrastructure.Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sitecore.DataExchange.Local.Extensions;

namespace Sitecore.Support.DataExchange.Providers.Sc.Processors.PipelineSteps
{
  public class ResolveSitecoreItemStepProcessorSitecore: Sitecore.DataExchange.Providers.Sc.Processors.PipelineSteps.ResolveSitecoreItemStepProcessor
  {
    private IValueReader GetValueReader(IValueAccessor config) =>
   config?.ValueReader;

    protected virtual string GetSearchIndexNameForDatabase(string databaseName) =>
$"sitecore_{databaseName}_index";




    protected override ItemModel DoSearch(object value, ResolveSitecoreItemSettings resolveItemSettings, IItemModelRepository repository, PipelineContext pipelineContext, ILogger logger)
    {
      var result = base.DoSearch(value, resolveItemSettings, repository, pipelineContext, logger);
      Sitecore.Data.Items.Item item;
      if (result == null)
      {
        SitecoreItemFieldReader valueReader = this.GetValueReader(resolveItemSettings.MatchingFieldValueAccessor) as SitecoreItemFieldReader;
        if (valueReader == null)
        {
          return null;
        }
        string str = this.ConvertValueForSearch(value);
        string FieldName = valueReader.FieldName;
        var database = Sitecore.Configuration.Factory.GetDatabase(repository.DatabaseName);
        var bucketItem = database.GetItem(Sitecore.Data.ID.Parse(resolveItemSettings.ParentItemIdItem));
        if (bucketItem != null && BucketManager.IsBucket(bucketItem))
        {
          try
          {
            string searchIndexNameForDatabase = this.GetSearchIndexNameForDatabase(repository.DatabaseName);
            var searchContext = ContentSearchManager.GetIndex(searchIndexNameForDatabase).CreateSearchContext();

            var searchResult = searchContext.GetQueryable<SearchResultItem>().Where(x => x[FieldName] == str).FirstOrDefault();
            if (searchResult != null)
            {
              item = searchResult.GetItem();
              Item[] items = { item };
              Sitecore.DataExchange.Repositories.SearchFilter filter1 = new Sitecore.DataExchange.Repositories.SearchFilter
              {
                FieldName = valueReader.FieldName,
                Value = str
              };
              ItemSearchSettings settings = new ItemSearchSettings();
              settings.SearchFilters.Add(filter1);
              return item.GetItemModel();

            }
          }
          catch (Exception e)
          {
            Sitecore.Diagnostics.Log.Error(e.Message, e);
            throw e;
          }
        }

      }
      return result;
    }
  }
}
