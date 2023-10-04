<?php // upgrade_printmod_statback.php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["username"])) {
    echo "2"; // Missing POST variable
    exit();
}

// Sanitize and Validate Inputs
$username = trim($_POST["username"]);

if (empty($username)) {
    echo "2"; // Missing POST variable
    exit();
}

try {
    // Begin transaction
    $pdo->beginTransaction();

    // Update or insert player's upgraded_printmod status
    $sql = "INSERT INTO player_stats (player_id, upgraded_printmod) VALUES ((SELECT id FROM players WHERE username = :username), 0) ON DUPLICATE KEY UPDATE upgraded_printmod = 0";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username, PDO::PARAM_STR);
    
    if ($stmt->execute()) {
        $pdo->commit();
        echo "0"; // Success
    } else {
        $pdo->rollBack();
        echo "1"; // Error
    }
} catch (PDOException $e) {
    $pdo->rollBack();
    echo "Database error: " . $e->getMessage();
    exit();
}
?>
