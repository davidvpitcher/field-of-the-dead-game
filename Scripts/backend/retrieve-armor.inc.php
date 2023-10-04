<?php // retrieve-armor.inc.php

$unity = true;
$login = true;
require_once '../../keys/storage.php';

// Check for missing username
if (!isset($_POST["username"])) {
    echo "Error: Missing username";
    exit();
}

// Sanitize the username input
$username = trim($_POST["username"]);

if (empty($username)) {
    echo "Error: Invalid username";
    exit();
}

try {
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

    // Get the player's armor setup
    $sql = "SELECT current_armor FROM player_stats WHERE player_id = :player_id";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':player_id', $player_id, PDO::PARAM_INT);
    $stmt->execute();
    
    $armor = $stmt->fetchColumn();

    if ($armor === false || $armor === NULL) {
        echo "NOTHING/NOTHING/NOTHING/NOTHING/NOTHING/NOTHING/";
    } else {
        echo $armor;
    }

} catch (PDOException $e) {
    echo "Database error: " . $e->getMessage();
    exit();
}
?>
