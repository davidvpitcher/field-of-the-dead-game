<?php // getAchievements.php
$unity = true; // required for the storage keys to function and get the right keys
$login = true; // required for the storage keys to function and get the right keys

require_once '../../keys/storage.php'; // this grants pdo and conn

// Check if playerID key exists in the POST data
if (!isset($_POST['playerID'])) {
    error_log("playerID key not found in POST data.");
    echo json_encode(["error" => "playerID not provided."]);
    exit();
}

$playerID = $_POST['playerID'];

// Input Validation
if (!is_numeric($playerID)) {
    error_log("Invalid input. playerID should be numeric.");
    echo json_encode(["error" => "Invalid input. playerID should be numeric."]);
    exit();
}

try {
    $stmt = $pdo->prepare("SELECT * FROM player_achievements WHERE playerID = ?");
    $stmt->execute([$playerID]);
    $achievements = $stmt->fetchAll();
    echo json_encode($achievements);
} catch (PDOException $e) {
    error_log("Database error: " . $e->getMessage());
    echo json_encode(["error" => "Database error"]);
}
?>
