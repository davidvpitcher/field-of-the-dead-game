<?php // get_stats.php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["username"])) {
    echo "2"; // Missing POST variable
    exit();
}

// Sanitize and Validate Input
$username = trim($_POST["username"]);

if (empty($username)) {
    echo "2"; // Invalid POST variable
    exit();
}

try {
    $sql = "SELECT upgraded_printmod, chosen_class, current_skin, unlocked_vehicle_terminal FROM player_stats WHERE player_id = (SELECT id FROM players WHERE username = :username)";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username, PDO::PARAM_STR);
    $stmt->execute();

    if ($stmt->rowCount() > 0) {
        $row = $stmt->fetch(PDO::FETCH_ASSOC);
        echo "0\t" . $row["upgraded_printmod"] . "\t" . $row["chosen_class"] . "\t" . $row["current_skin"] . "\t" . $row["unlocked_vehicle_terminal"];
    } else {
        echo "0\t0\t0\t0\t0";
    }
} catch (PDOException $e) {
    echo "Database error: " . $e->getMessage();
    exit();
}
?>
