# Azure OpenAI RAG Console Application

This application demonstrates how to build a Retrieval-Augmented Generation (RAG) system using Azure OpenAI and Azure Cognitive Search with enterprise-grade features including caching, validation, error handling, and performance monitoring.

## 🚀 Features

- **Flexible Data Sources**: Load documents from JSON files, CSV files, text files, or hardcoded data
- **Vector Search**: Uses Azure Cognitive Search with vector embeddings for semantic search
- **Interactive Chat**: Ask questions and get AI responses based on your knowledge base
- **Configurable**: Easy configuration through `appsettings.json`
- **🆕 Document Caching**: Intelligent caching system to improve performance
- **🆕 Enhanced Error Handling**: Detailed error messages and graceful fallbacks
- **🆕 Configuration Validation**: Validates configuration files and environment variables
- **🆕 Performance Monitoring**: Comprehensive performance logging and statistics
- **🆕 Robust File Handling**: Better file access validation and error recovery

## 📁 Data Source Configuration

The application supports multiple data source types configured in `appsettings.json`:

### 1. JSON Files (Default)

```json
{
  "DataSource": {
    "Type": "Json",
    "FilePath": "Data/documents.json"
  }
}
```

The JSON file should contain an array of documents:

```json
[
  {
    "Id": "1",
    "Title": "Document Title",
    "Content": "Document content here..."
  }
]
```

### 2. CSV Files

```json
{
  "DataSource": {
    "Type": "Csv",
    "FilePath": "Data/documents.csv"
  }
}
```

CSV format: `Id,Title,Content`

### 3. Text Files

```json
{
  "DataSource": {
    "Type": "TextFiles",
    "DirectoryPath": "Data/TextFiles"
  }
}
```

Each `.txt` file in the directory becomes a document. The filename becomes the title.

### 4. Hardcoded (Fallback)

```json
{
  "DataSource": {
    "Type": "HardCoded"
  }
}
```

Uses predefined documents in the code.

## 🛠️ Setup

### 🔐 Security Setup

1. **Copy the environment template:**

   ```bash
   cp .env.template .env
   ```

2. **Configure your Azure credentials in `.env`:**
   ```
   AOAI_ENDPOINT=https://your-openai-resource.openai.azure.com
   AOAI_APIKEY=your-azure-openai-api-key
   CHATCOMPLETION_DEPLOYMENTNAME=your-chat-deployment-name
   EMBEDDING_DEPLOYMENTNAME=your-embedding-deployment-name
   COGNITIVESEARCH_ENDPOINT=https://your-search-service.search.windows.net
   COGNITIVESEARCH_APIKEY=your-search-api-key
   ```

### ⚠️ Important Security Notes

- **Never commit `.env` to version control** - it's already in `.gitignore`
- Always keep your API keys private
- Regenerate keys if accidentally exposed
- Use Azure Key Vault for production deployments

### 📋 Quick Start Steps

```
AOAI_ENDPOINT=your_azure_openai_endpoint
AOAI_APIKEY=your_azure_openai_api_key
CHATCOMPLETION_DEPLOYMENTNAME=your_chat_deployment_name
EMBEDDING_DEPLOYMENTNAME=your_embedding_deployment_name
COGNITIVESEARCH_ENDPOINT=your_search_endpoint
COGNITIVESEARCH_APIKEY=your_search_api_key
### 📋 Quick Start Steps

1. **Copy environment template:** `cp .env.template .env`

2. **Configure your data source in `appsettings.json`**

3. **Add your documents** to the appropriate location based on your configuration

4. **Run the application:** `dotnet run`

## 🔄 How It Works

### RAG Process

1. **Document Loading**: Documents are loaded from your configured data source with caching
2. **Embedding Generation**: Each document is converted to vector embeddings using Azure OpenAI
3. **Index Creation**: Documents and embeddings are stored in Azure Cognitive Search
4. **Query Processing**: User questions are converted to embeddings
5. **Similarity Search**: Relevant documents are found using vector similarity
6. **Response Generation**: Azure OpenAI generates responses using the retrieved context

### Azure Search Integration

- Creates a search index with vector fields for semantic search
- Supports hybrid search (keyword + vector)
- Automatically handles embedding dimensions and vector search configuration

## 🚀 Performance Features

### Document Caching

- **Smart Caching**: Documents are cached for 5 minutes to improve performance
- **File Change Detection**: Cache is invalidated when source files are modified
- **Cache Statistics**: View cache usage and performance metrics

### Performance Monitoring

- **Operation Timing**: All major operations are timed automatically
- **Performance Statistics**: Detailed stats showing min/max/average times
- **Background Logging**: Non-intrusive performance data collection

Example performance output:

```

=== Performance Statistics ===
Load Configuration:
Count: 1
Total: 45ms
Average: 45.0ms
Min: 45ms
Max: 45ms

Generate Embedding:
Count: 6
Total: 1250ms
Average: 208.3ms
Min: 180ms
Max: 245ms
================================

````

## 🛡️ Error Handling & Validation

### Configuration Validation

- **Environment Variables**: Validates all required Azure credentials
- **File Paths**: Ensures data source files/directories exist
- **Index Names**: Validates Azure Search index naming conventions
- **Data Formats**: Validates JSON/CSV structure and content

### Enhanced Error Messages

- **File Access Issues**: Clear messages for permission and file locking problems
- **Data Format Errors**: Specific guidance on fixing JSON/CSV formatting issues
- **Missing Files**: Helpful suggestions when files are not found
- **Duplicate Data**: Detection and reporting of duplicate document IDs

### Graceful Fallbacks

- **Missing Configuration**: Uses sensible defaults when configuration is invalid
- **File Loading Errors**: Falls back to hardcoded data if external files fail
- **Individual File Errors**: Continues processing other files when one fails

## 💡 Benefits of the Enhanced System

### Development Benefits

- **🔍 Better Debugging**: Detailed error messages and performance data
- **⚡ Faster Iteration**: Caching reduces reload times during development
- **🛡️ Early Error Detection**: Configuration validation catches issues before deployment

### Production Benefits

- **📈 Performance Monitoring**: Track system performance and identify bottlenecks
- **🔒 Robust Error Handling**: Graceful degradation when components fail
- **🚀 Optimized Loading**: Caching reduces API calls and file I/O
- **📊 Operational Insights**: Performance statistics help with capacity planning

## 🎯 Example Usage

After running the application, you can ask questions like:

- "Tell me about the Eiffel Tower"
- "What museums are in Paris?"
- "How tall is the Eiffel Tower?"

The AI will respond based on the documents in your configured data source, and you'll see performance metrics for each operation.

## 🔧 Advanced Configuration

### Switching Data Sources Quickly

```bash
# Use JSON files (default)
cp appsettings.json appsettings.json

# Use text files
cp appsettings.textfiles.json appsettings.json

# Use CSV files
cp appsettings.csv.json appsettings.json
````

### Cache Management

The application automatically manages caching, but you can also:

- Clear cache manually (feature built-in)
- View cache statistics
- Adjust cache timeout (5 minutes default)

### Performance Tuning

- Monitor embedding generation times
- Track document loading performance
- Identify slow operations through statistics
- Optimize based on performance data

## 🏗️ Architecture Overview

```
┌─────────────────────────┐
│     Configuration       │
│   - appsettings.json    │
│   - Environment vars    │
│   - Validation          │
└─────────┬───────────────┘
          │
┌─────────▼───────────────┐
│    Document Loader      │
│   - Caching System      │
│   - Multiple Formats    │
│   - Error Handling      │
└─────────┬───────────────┘
          │
┌─────────▼───────────────┐
│   Azure OpenAI API      │
│   - Embedding Gen       │
│   - Chat Completion     │
│   - Performance Log     │
└─────────┬───────────────┘
          │
┌─────────▼───────────────┐
│  Azure Cognitive Search │
│   - Vector Index        │
│   - Hybrid Search       │
│   - Document Storage    │
└─────────────────────────┘
```

This enhanced RAG application provides a robust foundation for enterprise applications with comprehensive error handling, performance monitoring, and flexible data source management!
