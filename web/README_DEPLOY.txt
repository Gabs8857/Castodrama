FTP DEPLOY CHECKLIST

1) Upload everything from this folder to your web root.
2) Confirm PHP is enabled on hosting.
3) Open your site and verify index.html loads.
4) Test endpoint manually:
   https://YOUR_DOMAIN/contact.php?message=test
   Expected JSON: {"ok":true,"saved":true}
5) In Unity Inspector (ContacteWebPage), set formEndpoint to:
   https://YOUR_DOMAIN/contact.php

Notes:
- Messages are appended to /messages/messages.txt
- /messages is protected by .htaccess
- If SSL/certificate errors appear in Unity, fix domain certificate first.
