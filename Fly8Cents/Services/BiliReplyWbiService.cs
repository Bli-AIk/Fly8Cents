using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Fly8Cents.Services;

/// <summary>
/// Provides a service to sign parameters for Bilibili Comment API requests using the WBI algorithm.
/// This implementation is based on the algorithm observed for comment APIs, which uses a fixed salt.
/// </summary>
public class BiliCommentWbiService
{
    // This is the fixed salt used for signing comment API requests.
    // Extracted from the provided Python script.
    private const string WbiSalt = "ea1db124af3c7062474693fa704f4ff8";

    /// <summary>
    /// Signs a dictionary of parameters for a Bilibili comment API request.
    /// </summary>
    /// <param name="parameters">The parameters to sign.</param>
    /// <returns>A new dictionary containing the original parameters plus 'wts' and the calculated 'w_rid' signature.</returns>
    private Dictionary<string, string> Sign(Dictionary<string, string> parameters)
    {
        // Add the current Unix timestamp to the parameters.
        parameters["wts"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

        // The parameters must be sorted alphabetically by key before generating the query string.
        var sortedParams = new SortedDictionary<string, string>(parameters);

        // Create the query string by joining the key-value pairs.
        // Values are URL-encoded to match the Python script's behavior.
        var queryString = string.Join(
            "&",
            sortedParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}")
        );

        // Append the salt to the query string.
        var stringToHash = queryString + WbiSalt;

        // Calculate the MD5 hash of the resulting string.
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(stringToHash));

        // Convert the hash to a lowercase hexadecimal string.
        var wbiSign = Convert.ToHexStringLower(hashBytes);

        // Add the signature to the original parameters dictionary.
        parameters["w_rid"] = wbiSign;

        return parameters;
    }

    /// <summary>
    /// Asynchronously gives request parameters a WBI signature.
    /// The method is async to maintain a consistent interface with services that may perform network requests.
    /// </summary>
    /// <param name="parameters">The dictionary of parameters for the request.</param>
    /// <returns>A Task that resolves to the dictionary with the added 'w_rid' and 'wts' parameters.</returns>
    public Task<Dictionary<string, string>> SignAsync(Dictionary<string, string> parameters)
    {
        // The signing process is synchronous, so we wrap the result in a completed task.
        return Task.FromResult(Sign(parameters));
    }
}
