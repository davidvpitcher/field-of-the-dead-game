<?php // update-vehicle-access.inc.php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["player_id"])) {
    echo "Error: Missing player_id";
    exit();
}

$player_id = trim($_POST["player_id"]);

if (empty($player_id)) {
    echo "Error: Missing player_id";
    exit();
}

try {
    $pdo->beginTransaction();

    // Check if the row exists
    $sql = "SELECT COUNT(*) FROM player_stats WHERE player_id = :player_id";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':player_id', $player_id, PDO::PARAM_STR);
    $stmt->execute();
    $rowCount = $stmt->fetchColumn();

    if ($rowCount > 0) {
        $sql = "UPDATE player_stats SET unlocked_vehicle_terminal = 1 WHERE player_id = :player_id";
    } else {
        // Insert a new row if it doesn't exist
        $sql = "INSERT INTO player_stats (player_id, unlocked_vehicle_terminal) VALUES (:player_id, 1)";
    }

    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':player_id', $player_id, PDO::PARAM_STR);
    $success = $stmt->execute();

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
