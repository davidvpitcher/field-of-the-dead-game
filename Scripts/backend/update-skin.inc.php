<?php // update-skin.inc.php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["username"]) || !isset($_POST["current_skin"])) {
    echo "Error: Missing username or current_skin";
    exit();
}

// Sanitize and Validate Inputs
$username = trim($_POST["username"]);
$current_skin = trim($_POST["current_skin"]);

if (empty($username) || empty($current_skin)) {
    echo "Error: Missing username or current_skin";
    exit();
}

try {
    // Begin transaction
    $pdo->beginTransaction();

    // Get the player's ID
    $sql = "SELECT id FROM players WHERE username = :username";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username, PDO::PARAM_STR);
    $stmt->execute();
    $player_id = $stmt->fetchColumn();

    if (!$player_id) {
        echo "Error: Player not found.";
        exit();
    }

    // Try to update the player's current skin
    $sql = "UPDATE player_stats SET current_skin = :current_skin WHERE player_id = :player_id";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':player_id', $player_id, PDO::PARAM_INT);
    $stmt->bindValue(':current_skin', $current_skin, PDO::PARAM_STR);
    $success = $stmt->execute();
    
    if (!$success) {
        // Update failed, try to insert a new row
        $sql = "INSERT INTO player_stats (player_id, current_skin) VALUES (:player_id, :current_skin)";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':player_id', $player_id, PDO::PARAM_INT);
        $stmt->bindValue(':current_skin', $current_skin, PDO::PARAM_STR);
        $success = $stmt->execute();
    }
    
    if ($success) {
        $pdo->commit();
        echo "0"; // Success code
    } else {
        $pdo->rollBack();
        echo "Error: Execution failed.";
    }
} catch (PDOException $e) {
    $pdo->rollBack();
    echo "Database error: " . $e->getMessage();
    exit();
}
