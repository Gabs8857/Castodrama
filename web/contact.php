<?php
header('Content-Type: application/json; charset=UTF-8');

// Accept both GET (?message=...) and POST (message=...)
$rawMessage = '';
if (isset($_POST['message'])) {
    $rawMessage = $_POST['message'];
} elseif (isset($_GET['message'])) {
    $rawMessage = $_GET['message'];
}

$message = trim((string)$rawMessage);
if ($message === '') {
    http_response_code(400);
    echo json_encode(['ok' => false, 'error' => 'message empty']);
    exit;
}

if (strlen($message) > 1000) {
    http_response_code(413);
    echo json_encode(['ok' => false, 'error' => 'message too long']);
    exit;
}

$ip = $_SERVER['REMOTE_ADDR'] ?? 'unknown';
$timestamp = date('Y-m-d H:i:s');
$singleLineMessage = str_replace(["\r", "\n"], ' ', $message);
$line = $timestamp . ' | ' . $ip . ' | ' . $singleLineMessage . PHP_EOL;

$storageDir = __DIR__ . '/messages';
if (!is_dir($storageDir)) {
    mkdir($storageDir, 0755, true);
}

$storageFile = $storageDir . '/messages.txt';
$writeResult = @file_put_contents($storageFile, $line, FILE_APPEND | LOCK_EX);
if ($writeResult === false) {
    http_response_code(500);
    echo json_encode(['ok' => false, 'error' => 'write failed']);
    exit;
}

echo json_encode(['ok' => true, 'saved' => true]);
