<?php
if (isset($_POST["username"]) && isset($_POST["gainLoss"])) {
    $username = $_POST["username"];
    $gainLoss = intval($_POST["gainLoss"]);

    $unity = true;
    $login = true;

    require_once '../../keys/storage.php';

    // Begin transaction
    $pdo->beginTransaction();

    $sql = "UPDATE player_stats
            SET net_gain_loss = net_gain_loss + :gainLoss
            WHERE player_id = (SELECT id FROM players WHERE username = :username);";

    try {
        $stmt = $pdo->prepare($sql);
        $stmt->bindParam(':gainLoss', $gainLoss, PDO::PARAM_INT);
        $stmt->bindParam(':username', $username, PDO::PARAM_STR);
        $success = $stmt->execute();

        if (!$success) {
            // Update failed, try to insert a new row
            $sql = "INSERT INTO player_stats (player_id, net_gain_loss) VALUES ((SELECT id FROM players WHERE username = :username), :gainLoss)";
            $stmt = $pdo->prepare($sql);
            $stmt->bindParam(':username', $username, PDO::PARAM_STR);
            $stmt->bindParam(':gainLoss', $gainLoss, PDO::PARAM_INT);
            $success = $stmt->execute();
        }

        if ($success) {
            // Commit transaction
            $pdo->commit();
            echo "0"; // success code
        } else {
            // Rollback transaction
            $pdo->rollBack();
            echo "1"; // error code
        }

        exit();
    } catch (PDOException $e) {
        // Rollback transaction
        $pdo->rollBack();
        echo "1"; // error code
        exit();
    }
} else {
    echo "2"; // error code for missing POST variable
    exit();
}
?>
