<?php // update_chosen_class_stat.php
$unity = true;
$login = true;
require_once '../../keys/storage.php';

if (!isset($_POST["username"]) || !isset($_POST["class_stat"])) {
    echo "2"; // Missing POST variables
    exit();
}

// Sanitize and Validate Inputs
$username = trim($_POST["username"]);
$class_stat = intval($_POST["class_stat"]); // already sanitized through intval

if (empty($username) || $class_stat < 0) {
    echo "2"; // Missing or invalid POST variables
    exit();
}

try {
    // Begin transaction
    $pdo->beginTransaction();

    // Try to update the player_stats row
    $sql = "UPDATE player_stats SET chosen_class = :class_stat WHERE player_id = (SELECT id FROM players WHERE username = :username)";
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username, PDO::PARAM_STR);
    $stmt->bindValue(':class_stat', $class_stat, PDO::PARAM_INT);

    if ($stmt->execute() && $stmt->rowCount() > 0) {
        $pdo->commit();
        echo "0"; // success
    } else {
        // If the update fails, insert a new row with chosen_class set to the value of class_stat
        $sql = "INSERT INTO player_stats (player_id, chosen_class) VALUES ((SELECT id FROM players WHERE username = :username), :class_stat)";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':username', $username, PDO::PARAM_STR);
        $stmt->bindValue(':class_stat', $class_stat, PDO::PARAM_INT);

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
