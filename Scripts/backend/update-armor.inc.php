<?php // update-armor.inc.php

$unity = true;
$login = true;
require_once '../../keys/storage.php';

// Check for missing username or armor
if (!isset($_POST["username"]) || !isset($_POST["armor"])) {
    echo "Error: Missing username or armor";
    exit();
}

// Sanitize inputs
$username = trim($_POST["username"]);
$armor = trim($_POST["armor"]);

if (empty($username) || empty($armor)) {
    echo "Error: Invalid username or armor";
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

    // Try to update the player's armor
    $sql = "UPDATE player_stats SET current_armor = :armor WHERE player_id = :player_id";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':player_id', $player_id, PDO::PARAM_INT);
    $stmt->bindValue(':armor', $armor, PDO::PARAM_STR);
    
    $success = $stmt->execute();

    if (!$success) {
        // Update failed, try to insert a new row
        $sql = "INSERT INTO player_stats (player_id, current_armor) VALUES (:player_id, :armor)";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':player_id', $player_id, PDO::PARAM_INT);
        $stmt->bindValue(':armor', $armor, PDO::PARAM_STR);
        
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
?>
