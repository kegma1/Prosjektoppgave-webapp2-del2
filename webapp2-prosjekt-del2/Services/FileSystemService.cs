using System.Net.Http.Headers;
using System.Text.Json.Serialization;

public class FileSystemService {
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, Folder> _fileSystem = new();
    private readonly Dictionary<string, Document> _documents = new();

    private bool _isFolderSystemInitialized = false;
    private bool _isDocumentsInitialized = false;

    public FileSystemService(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public IReadOnlyDictionary<string, Folder> FileSystem => _fileSystem;
    public IReadOnlyDictionary<string, Document> Documents => _documents;

    public async Task InitializeFileSystemAsync(string token) {
        await InitFolderSystem(token);
        await InitDocuments(token);
    }

    private async Task InitFolderSystem(string token) {
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
            _isFolderSystemInitialized = true;
        }
    }

    private async Task InitDocuments(string token) {
        _documents.Clear();

        var req = new HttpRequestMessage(HttpMethod.Get, "Document");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _httpClient.SendAsync(req);
        res.EnsureSuccessStatusCode();

        var documents = await res.Content.ReadFromJsonAsync<List<Document>>();
        if (documents != null) {
            foreach (var document in documents) {
                string VirtualPath = GetVirtualPath(document);
                Console.WriteLine(VirtualPath);
                _documents[VirtualPath] = document;
            }
            _isDocumentsInitialized = true;
        }
    }

    

    public string GetVirtualPath(IFile file) {
        if (file.parentFolder != null) {
            return  GetVirtualPath(file.parentFolder) + "/" + file.name;
        } else {
            return "/" + file.name;
        }
    }

    public Folder? GetFolder(string path) {
        return _fileSystem.ContainsKey(path) ? _fileSystem[path] : null;
    }
    
    public Document? GetDocument(string path) {
        return _documents.ContainsKey(path) ? _documents[path] : null;
    }

    public IFile? GetFile(string path) {
        return (IFile?)GetDocument(path) ?? GetFolder(path);
    }

    public bool IsInitialized() {
        return _isFolderSystemInitialized && _isDocumentsInitialized;
    }
}

public interface IFile {
    public int id { get; set; }
    public string name { get; set; }
    public int? parentFolderId { get; set; }
    public Folder? parentFolder { get; set; }

    public string endpoint => "";
}

public class Folder : IFile {
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public int? parentFolderId { get; set; }
    public Folder? parentFolder { get; set; }

    public string endpoint => "Folder";
}

public class Document : IFile {
    public int id { get; set; }
    
    [JsonPropertyName("title")]
    public string name { get; set; } = string.Empty;    
    public string content { get; set; } = string.Empty;    
    public int? parentFolderId { get; set; }
    public Folder? parentFolder { get; set; }

    [JsonPropertyName("createdDate")]
    public DateTime createdDate { get; set; }
    public int contentTypeId { get; set; }
    public string endpoint => "Document";
}