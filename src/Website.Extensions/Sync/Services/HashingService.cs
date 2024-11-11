using SoleMates.Website.Extensions.Fetch.Models;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core.Models;

namespace SoleMates.Website.Extensions.Fetch.Services;
public class HashingService {
  public string GetFormattedHash(SeriesModel model) {
    string concatination = $"{model.Brand}{model.Name}{model.Price}";
    byte[] hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(concatination));
    string unformattedHash = BitConverter.ToString(hashBytes);         //Unformatted looks like this:
    string formattedHash = unformattedHash.Replace("-", "").ToLower(); //5E-B6-3B-BB-E0-1E-EE-D0-93-CB-22-BB-8F-5A-CD-C3

    return formattedHash;
  }

  /// <summary>
  /// Takes an <see cref="IContent"/> node, that contains the last stored hash, and a <see cref="SeriesModel"/> that represents <br/>
  /// a 'Series' from the latest fetch. <br/>
  /// It throws a <see cref="NullReferenceException"/> if the passed node does not contain a property of string "hash". <br/>
  /// The exception is handled by HangFire and will be displayed in the dashboard under "retries".
  /// </summary>
  /// <exception cref="NullReferenceException"></exception>
  public bool TryCompareHashes(IContent lastHashNode, SeriesModel currentHashModel) {
    string? lastHash = lastHashNode.GetValue<string>("hash");
    string currentHash = GetFormattedHash(currentHashModel);

    if (string.IsNullOrWhiteSpace(lastHash)) {
      throw new NullReferenceException("HashingService.TryCompareHashes(): lastHash is null or empty.");
    }

    return string.Equals(lastHash, currentHash);
  }
}
