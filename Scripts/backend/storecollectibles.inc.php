<?php // storecollectibles.inc.php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["username"]) || !isset($_POST["collectiblesData"])) {
    echo "7: uh oh"; // Missing or invalid POST variables
    exit();
}

// Sanitize and validate input
$username = trim($_POST["username"]);
$collectiblesData = trim($_POST["collectiblesData"]);

if (empty($username) || empty($collectiblesData)) {
    echo "7: uh oh"; // Invalid POST variables
    exit();
}

// Start by getting the player_id
$stmt = $conn->prepare("SELECT `id` FROM `players` WHERE `username` = ?");
$stmt->bind_param("s", $username);
$stmt->execute();
$result = $stmt->get_result();
$row = $result->fetch_assoc();
$player_id = $row["id"];

if (!$player_id) {
    echo "7: uh oh"; // User not found
    exit();
}

$stmt = $conn->prepare("SELECT * FROM `player_stats` WHERE `player_id` = ?");
$stmt->bind_param("i", $player_id);
$stmt->execute();
$result = $stmt->get_result();

if ($result->num_rows == 0) {
    // Insert new row if it does not exist
    $stmt = $conn->prepare("INSERT INTO `player_stats` (`player_id`, `collectiblesData`) VALUES (?, ?)");
    $stmt->bind_param("is", $player_id, $collectiblesData);
} else {
    // Update existing row
    $stmt = $conn->prepare("UPDATE `player_stats` SET `collectiblesData` = ? WHERE `player_id` = ?");
    $stmt->bind_param("si", $collectiblesData, $player_id);
}

if ($stmt->execute()) {
    echo "0"; // Success
} else {
    echo "7: uh oh"; // Query execution error
}

// Close database connection
$conn->close();
exit();
?>
