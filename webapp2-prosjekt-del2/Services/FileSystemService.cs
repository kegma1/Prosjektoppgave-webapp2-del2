using System.Net.Http.Headers;

public class FileSystemService {
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, Folder> _fileSystem = new();

    private bool _isInitialized = false;

    public FileSystemService(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public IReadOnlyDictionary<string, Folder> FileSystem => _fileSystem;

    public async Task InitializeFileSystemAsync(string token) {
        _fileSystem.Clear();

        var req = new HttpRequestMessage(HttpMethod.Get, "Folder");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _httpClient.SendAsync(req);
        res.EnsureSuccessStatusCode();

        var folders = await res.Content.ReadFromJsonAsync<List<Folder>>();
        if (folders != null) {
            foreach (var folder in folders) {
                string VirtualPath = GetVirtualPath(folder);
                _fileSystem[VirtualPath] = folder;
            }
            _isInitialized = true;
        }

    }

    private string GetVirtualPath(Folder folder) {
        if (folder.parentFolder != null) {
            return  GetVirtualPath(folder.parentFolder) + "/" + folder.name;
        } else {
            return "/" + folder.name;
        }
    }

    public Folder? GetFolder(string path) {
        return _fileSystem.ContainsKey(path) ? _fileSystem[path] : null;
    }

    public bool IsInitialized() {
        return _isInitialized;
    }
}

public class Folder {
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public int? parentFolderId { get; set; }
    public Folder? parentFolder { get; set; }
}

public class Document {
    public int id { get; set; }
    public string title { get; set; } = string.Empty;    
    public int? parentFolderId { get; set; }
    public Folder? parentFolder { get; set; }
    public DateTime createdDate { get; set; }

}