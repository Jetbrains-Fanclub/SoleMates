using Microsoft.Extensions.Logging;
using SoleMates.Website.Extensions.Sync.Models;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core.Models;

namespace SoleMates.Website.Extensions.Sync.Services;
public class HashingService {
  private readonly ILogger<HashingService> _logger;

  public HashingService(ILogger<HashingService> logger) {
    _logger = logger;
  }

  /// <summary> Creates a formatted MD5 hash based on the passed models 'Brand','Series' and 'Price' properties.</summary>
  public string GetFormattedHash(SeriesModel model) {
    string concatination = $"{model.Brand}{model.Name}{model.Price}";
    byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(concatination));
    string unformattedHash = BitConverter.ToString(hashBytes);         //Unformatted output looks like this:
    string formattedHash = unformattedHash.Replace("-", "").ToLower(); //5E-B6-3B-BB-E0-1E-EE-D0-93-CB-22-BB-8F-5A-CD-C3

    return formattedHash;
  }

  /// <summary> Takes an <see cref="IContent"/> node, that contains the last stored hash, and a <see cref="SeriesModel"/> that represents a 'Series' from the latest fetch. <br/>
  /// If the "last hash" is not found, this is logged, and <see langword="false"/> is returned, which updates the node with the newest data from the ERP. </summary>
  public bool TryCompareHashes(IContent lastHashNode, SeriesModel currentHashModel) {
    string? lastHash = lastHashNode.GetValue<string>("hash");
    string currentHash = GetFormattedHash(currentHashModel);

    if (string.IsNullOrWhiteSpace(lastHash)) {
      _logger.LogWarning($"HashingService.TryCompareHashes() - Could not find the property 'hash'. Returned false, which updates the node.");
      return false;
    }

    return string.Equals(lastHash, currentHash);
  }
}
