<?php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["username"])) {
    echo "7: uh oh"; // Missing POST variables
    exit();
}

// Sanitize and validate input
$username = trim($_POST["username"]);

if (empty($username)) {
    echo "7: uh oh"; // Invalid POST variables
    exit();
}

// Get the player_id
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

// Get the collectiblesData
$stmt = $conn->prepare("SELECT `collectiblesData` FROM `player_stats` WHERE `player_id` = ?");
$stmt->bind_param("i", $player_id);
$stmt->execute();
$result = $stmt->get_result();

if ($result->num_rows > 0) {
    $row = $result->fetch_assoc();
    echo $row["collectiblesData"]; // Output the collectiblesData
} else {
    echo "0"; // No entry exists
}

// Close the database connection
$conn->close();
exit();
?>
