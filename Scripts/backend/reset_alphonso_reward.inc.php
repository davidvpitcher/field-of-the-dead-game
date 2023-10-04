<?php // reset_alphonso_reward.inc.php
if (isset($_POST["playerID"])) {
    $playerID = $_POST["playerID"];
    
    $unity = true;
    $login = true;
    require_once '../../keys/storage.php';
    
    $pdo->beginTransaction();
    
    $sql = "INSERT INTO player_stats (player_id, received_alphonso_upgrade) VALUES (:playerID, 0) ON DUPLICATE KEY UPDATE received_alphonso_upgrade = 0";
    
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':playerID', $playerID, PDO::PARAM_INT);
    
    try {
        if ($stmt->execute()) {
            $pdo->commit();
            echo "0"; // success
        } else {
            $pdo->rollBack();
            echo "1"; // error
        }
    } catch (PDOException $e) {
        $pdo->rollBack();
        echo "1"; // could log this error
    }
} else {
    echo "2"; // missing POST variable
}
