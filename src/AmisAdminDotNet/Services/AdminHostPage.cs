namespace AmisAdminDotNet.Services;

public static class AdminHostPage
{
    public static string Html => RenderHtml(new AppSettings());

    public static string RenderHtml(AppSettings settings)
    {
        var amisCdn = string.IsNullOrWhiteSpace(settings.AmisCdn)
            ? new AppSettings().AmisCdn
            : settings.AmisCdn.TrimEnd('/');
        var schemaApiPath = string.IsNullOrWhiteSpace(settings.SchemaApiPath)
            ? "/api/admin/schema"
            : settings.SchemaApiPath;

        return $$"""
<!doctype html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Amis Admin .NET Core</title>
  <link rel="stylesheet" href="{{amisCdn}}/sdk.css" />
  <link rel="stylesheet" href="{{amisCdn}}/helper.css" />
  <link rel="stylesheet" href="{{amisCdn}}/iconfont.css" />
  <link rel="stylesheet" href="/admin/site.css" />
</head>
<body>
  <div id="root"></div>
  <script src="https://unpkg.com/react@18/umd/react.production.min.js"></script>
  <script src="https://unpkg.com/react-dom@18/umd/react-dom.production.min.js"></script>
  <script src="{{amisCdn}}/sdk.js"></script>
  <script>
    function escapeHtml(value) {
      return String(value)
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#39;');
    }

    function renderFallback(schema, reason) {
      fetch('/api/admin/users?page=1&perPage=5')
        .then(function (response) { return response.json(); })
        .then(function (result) {
          var users = (result.data && result.data.items) || [];
          var rows = users.map(function (user) {
            return '<tr>' +
              '<td>' + escapeHtml(user.id) + '</td>' +
              '<td>' + escapeHtml(user.name) + '</td>' +
              '<td>' + escapeHtml(user.email) + '</td>' +
              '<td>' + escapeHtml(user.role) + '</td>' +
              '<td>' + escapeHtml(user.enabled ? 'Enabled' : 'Disabled') + '</td>' +
              '</tr>';
          }).join('');

          document.getElementById('root').innerHTML =
            '<div class="fallback">' +
              '<h1>Amis Admin .NET Core</h1>' +
              '<p>This environment cannot load the external amis CDN, so a backend-driven fallback preview is shown below.</p>' +
              '<div class="notice">' + escapeHtml(reason) + '</div>' +
              '<h2>Users preview</h2>' +
              '<table>' +
                '<thead><tr><th>ID</th><th>Name</th><th>Email</th><th>Role</th><th>Status</th></tr></thead>' +
                '<tbody>' + rows + '</tbody>' +
              '</table>' +
              '<h2>Schema preview</h2>' +
              '<pre>' + escapeHtml(JSON.stringify(schema, null, 2)) + '</pre>' +
            '</div>';
        })
        .catch(function (error) {
          document.getElementById('root').innerHTML =
            '<pre style="padding:16px;color:#c00;">Failed to render fallback preview: ' +
            escapeHtml(error) +
            '</pre>';
        });
    }

    fetch('{{schemaApiPath}}')
      .then(function (response) { return response.json(); })
      .then(function (schema) {
        if (typeof amisRequire === 'function') {
          var amis = amisRequire('amis/embed');
          amis.embed('#root', schema, {}, { theme: 'cxd' });
          return;
        }

        renderFallback(schema, 'Official amis SDK could not be loaded in this environment.');
      })
      .catch(function (error) {
        document.getElementById('root').innerHTML =
          '<pre style="padding:16px;color:#c00;">Failed to load amis schema: ' +
          error +
          '</pre>';
      });
  </script>
</body>
</html>
""";
    }
}
