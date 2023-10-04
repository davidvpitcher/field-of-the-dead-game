<?php // update_printmod_stat.php
$unity = true; // required to access the keys
$login = true; // required to access the keys
require_once '../../keys/storage.php'; // required to access the conn and pdo

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

    // Try to update the player_stats row
    $sql = "UPDATE player_stats SET upgraded_printmod = 1 WHERE player_id = (SELECT id FROM players WHERE username = :username)";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username, PDO::PARAM_STR);

    if ($stmt->execute() && $stmt->rowCount() > 0) {
        $pdo->commit();
        echo "0"; // success
    } else {
        // If the update fails, insert a new row with upgraded_printmod set to 1
        $sql = "INSERT INTO player_stats (player_id, upgraded_printmod) VALUES ((SELECT id FROM players WHERE username = :username), 1)";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':username', $username, PDO::PARAM_STR);

        if ($stmt->execute()) {
            $pdo->commit();
            echo "0"; // success
        } else {
            $pdo->rollBack();
            echo "1"; // error
        }
    }
} catch (PDOException $e) {
    $pdo->rollBack();
    echo "Database error: " . $e->getMessage();
    exit();
}
?>
